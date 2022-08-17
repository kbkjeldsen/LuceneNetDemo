using LuceneDemo.WebApi.Models;

namespace LuceneDemo.WebApi.Search
{
    public interface ICustomersSearchService
    {
        Task RefreshCustomersSearchIndexAsync();

        /// <summary>
        /// Performs a search for customers macthing a specific search term.
        /// </summary>
        /// <param name="searchTerm">The term to search for.</param>
        /// <param name="numberOfResults">Threshold for the maximum number of results if more results are found.</param>
        /// <returns></returns>
        IEnumerable<ISimpleCustomer> SearchForCustomers(string searchTerm, int numberOfResults);
    }
}
