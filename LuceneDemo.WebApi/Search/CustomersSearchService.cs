using LuceneDemo.WebApi.Models;

namespace LuceneDemo.WebApi.Search
{
    public class CustomersSearchService : ICustomersSearchService
    {
        private readonly ICustomersSearchEngine _searchEngine;

        public CustomersSearchService(ICustomersSearchEngine searchEngine)
        {
            this._searchEngine = searchEngine;
        }

        public Task RefreshCustomersSearchIndexAsync()
        {
            return this._searchEngine.RebuildIndexAsync();
        }

        public IEnumerable<ISimpleCustomer> SearchForCustomers(string searchTerm, int numberOfResults)
        {
            var documents = this._searchEngine.DoSearch(searchTerm, numberOfResults);

            var customers = documents
                .Select(doc =>
                    new CustomerDto(
                        doc.Get(CustomersSearchEngine.CUSTOMER_KEY_FIELDNAME),
                        doc.Get(CustomersSearchEngine.FULL_NAME_FIELDNAME)
                    ));

            // Return customers;
            return customers.AsEnumerable();
        }
    }
}

