using Lucene.Net.Documents;

namespace LuceneDemo.WebApi.Search
{
    public interface ICustomersSearchService
    {
        /// <summary>
        /// Bulds the customers search index.
        /// </summary>
        void BuildIndex();

        /// <summary>
        /// Performs a search for indexed customers macthing a specific search term. 
        /// </summary>
        /// <param name="searchTerm">The term to search for.</param>
        /// <param name="numberOfResults">Threshold for the maximum number of results if more results are found.</param>
        /// <returns></returns>
        IEnumerable<Document> DoSearch(string searchTerm, int numberOfResults);
    }
}
