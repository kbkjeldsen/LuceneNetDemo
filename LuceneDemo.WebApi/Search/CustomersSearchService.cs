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
    public class CustomersSearchService : ICustomersSearchService
    {
        // Specify the compatibility version we want
        private const LuceneVersion LUCENE_VERSION = LuceneVersion.LUCENE_48;

        private readonly IServiceScopeFactory _scopeFactory;

        private readonly string _indexName = "Lucene_INDEX/Customers";
        private readonly string _indexPath;

        private readonly Analyzer _analyzer;
        private readonly LuceneDirectory _indexDir;
        private readonly IndexWriterConfig _indexConfig;
        private readonly IndexWriter _writer;

        public CustomersSearchService(IServiceScopeFactory scopeFactory)
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
                OpenMode = OpenMode.CREATE // create/overwrite index
            };

            // Create an index writer
            this._writer = new IndexWriter(this._indexDir, this._indexConfig);
        }

        public void BuildIndex()
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

            Console.WriteLine("Start indexing the customers ...");

            sw.Restart();

            // Irerate the customers fetched from the database and add an entry to the search index for each customer.
            foreach (var customer in customersToIndex)
            {
                //Console.WriteLine($"Processing customer: {customer.CustomerKey}, {customer.FullName}");

                this._writer.AddDocument(
                    new Document
                    {
                        new StringField("customerKey", customer.CustomerKey, Field.Store.YES),
                        new TextField("fullName", customer.FullName, Field.Store.YES)
                    }
                );
            }

            //Flush and commit the index data to the directory
            this._writer.Commit();

            sw.Stop();

            Console.WriteLine($"{customersToIndex.Count} customers has been indexed from the database in {((double)sw.ElapsedMilliseconds / 1000):0.00} seconds.");

            Console.WriteLine("\n=== Finished building the search index for customers ===\n");
        }

        public IEnumerable<Document> DoSearch(string searchTerm, int numberOfResults)
        {
            if (String.IsNullOrEmpty(searchTerm))
            {
                return Enumerable.Empty<Document>();
            }

            DirectoryReader reader = this._writer.GetReader(applyAllDeletes: true);
            IndexSearcher searcher = new IndexSearcher(reader);
            Sort resultsSortOrder = new Sort(SortField.FIELD_SCORE);

            Console.WriteLine($"Searching for customers matching the pattern: {searchTerm}");

            string FIELD_NAME = "fullName";
            QueryParser parser = new QueryParser(LUCENE_VERSION, FIELD_NAME, this._analyzer);
            // Construct separate queries with various match methods to use for the search
            var exactQuery = parser.Parse(searchTerm);
            var wildCardQuery = new WildcardQuery(term: new Term(FIELD_NAME, $"{searchTerm}*"));
            var fuzzyQuery = new FuzzyQuery(term: new Term(FIELD_NAME, searchTerm), maxEdits: 2);
            // Combine the various queries into one uniform query that takes all the various query methods into consideration
            var combinedQuery = new BooleanQuery()
            {
                { exactQuery, Occur.SHOULD },
                { wildCardQuery, Occur.SHOULD },
                { fuzzyQuery, Occur.SHOULD }
            };

            // Do the search and find the desired number of results
            TopDocs topDocs = searcher.Search(query: combinedQuery, n: numberOfResults, sort: resultsSortOrder); //indicate we want the first X results

            int matchCount = topDocs.TotalHits;
            Console.WriteLine($"The search resulted in {matchCount} macthing customers.");

            return topDocs.ScoreDocs.Select(scoreDoc => searcher.Doc(scoreDoc.Doc)).AsEnumerable();
        }
    }
}
