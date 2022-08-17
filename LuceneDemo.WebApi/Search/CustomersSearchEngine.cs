using System.Diagnostics;

using Customers.Data;

using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;

using LuceneDirectory = Lucene.Net.Store.Directory;

namespace LuceneDemo.WebApi.Search
{
    public class CustomersSearchEngine : ICustomersSearchEngine
    {
        /* Field names for Lucene Documents containing customer data */
        public const string CUSTOMER_KEY_FIELDNAME = "CustomerKey";
        public const string FULL_NAME_FIELDNAME = "FullName";

        // Specify the compatibility version we want
        private const LuceneVersion LUCENE_VERSION = LuceneVersion.LUCENE_48;

        private readonly IServiceScopeFactory _scopeFactory;

        private readonly string _indexName = "Lucene_INDEX/Customers";
        private readonly string _indexPath;

        private readonly Analyzer _analyzer;
        private readonly LuceneDirectory _indexDir;
        private readonly IndexWriterConfig _indexConfig;
        private readonly IndexWriter _writer;
        private readonly SearcherManager _searcherManager;

        public CustomersSearchEngine(IServiceScopeFactory scopeFactory)
        {
            this._scopeFactory = scopeFactory;

            this._indexPath = Path.Combine(Environment.CurrentDirectory, this._indexName);
            Console.WriteLine($"Using directory for index: {this._indexPath}");

            // Open the Directory using a Lucene Directory class
            this._indexDir = FSDirectory.Open(this._indexPath);

            // Create an analyzer to process the text
            this._analyzer = new StandardAnalyzer(LUCENE_VERSION);

            this._indexConfig = new IndexWriterConfig(LUCENE_VERSION, this._analyzer)
            {
                // Create a new index if one does not already exist, otherwise open the index and documents will be appended.
                OpenMode = OpenMode.CREATE_OR_APPEND
            };

            // Create an index writer instance
            this._writer = new IndexWriter(this._indexDir, this._indexConfig);

            // Create a searcher manager instance
            this._searcherManager = new SearcherManager(this._writer, false, null);
        }

        public Task RebuildIndexAsync()
        {
            this.RebuildIndex();
            return Task.CompletedTask;
        }

        public void RebuildIndex()
        {
            Console.WriteLine("\n=== Start building the search index for customers ===\n");

            // Create a new scope (since DbContext is scoped by default)
            using var scope = this._scopeFactory.CreateScope();

            // Get a Dbcontext from the scope
            var customersDbContext = scope.ServiceProvider.GetRequiredService<CustomersDbContext>();

            Console.WriteLine("Start fetching customers from the database");

            var sw = Stopwatch.StartNew();

            // Fetch the customers to index from the db context
            var customersToIndex = customersDbContext.Customers
                .ToList();

            //sw.Stop();

            Console.WriteLine($"{customersToIndex.Count} customers has been fetched from the database in {((double)sw.ElapsedMilliseconds / 1000):0.00} seconds.");

            sw.Restart();

            // Remove all current documents from the index - this will not be effective until a Commit() has been called on the writer.
            this._writer.DeleteAll();

            Console.WriteLine("Start indexing the customers ...");

            this._writer.AddDocuments(
                customersToIndex.Select(customer => new Document {
                    new StringField(CUSTOMER_KEY_FIELDNAME, customer.CustomerKey, Field.Store.YES),
                    new TextField(FULL_NAME_FIELDNAME, customer.FullName, Field.Store.YES)
                }));

            //Flush and commit the index data to the directory
            this._writer.Commit();

            sw.Stop();

            Console.WriteLine($"{customersToIndex.Count} customers has been indexed from the database in {((double)sw.ElapsedMilliseconds / 1000):0.00} seconds.");

            Console.WriteLine("\n=== Finished building the search index for customers ===\n");
        }

        public IEnumerable<Document> DoSearch(string searchTerm, int numberOfResults)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return Enumerable.Empty<Document>();
            }

            // Make sure that the reference manager is returning a refreshed instance  
            this._searcherManager.MaybeRefreshBlocking();

            IndexSearcher searcher = this._searcherManager.Acquire();

            Sort resultsSortOrder = new Sort(SortField.FIELD_SCORE);

            QueryParser parser = new QueryParser(LUCENE_VERSION, FULL_NAME_FIELDNAME, this._analyzer);

            // Construct separate queries with various match methods to use for the search
            var exactQuery = parser.Parse(searchTerm);
            var wildCardQuery = new WildcardQuery(term: new Term(FULL_NAME_FIELDNAME, $"{searchTerm}*"));
            var fuzzyQuery = new FuzzyQuery(term: new Term(FULL_NAME_FIELDNAME, searchTerm), maxEdits: 2);

            // Combine the various queries into one uniform query that takes all the various query methods into consideration
            var combinedQuery = new BooleanQuery()
            {
                { exactQuery, Occur.SHOULD },
                { wildCardQuery, Occur.SHOULD },
                { fuzzyQuery, Occur.SHOULD }
            };

            try
            {
                Console.WriteLine($"Searching for customers matching the pattern: {searchTerm}");

                // Do the search and find the desired number of results
                TopDocs topDocs = searcher.Search(query: combinedQuery, n: numberOfResults, sort: resultsSortOrder); //indicate we want the first X results

                int matchCount = topDocs.TotalHits;
                Console.WriteLine($"The search resulted in {matchCount} macthing customers.");

                var docsToReturn = topDocs.ScoreDocs.Select(scoreDoc => searcher.Doc(scoreDoc.Doc)).ToList();

                return docsToReturn;
            }
            finally
            {
                this._searcherManager.Release(searcher);
                searcher = null;
            }
        }
    }
}
