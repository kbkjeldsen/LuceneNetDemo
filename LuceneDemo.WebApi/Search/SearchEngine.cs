
using LuceneDemo.WebApi.Search;

namespace LuceneDemo.WebApi
{
    public class SearchEngine
    {
        private readonly ICustomersSearchService _customersSearch;

        public SearchEngine(ICustomersSearchService customersSearch)
        {
            this._customersSearch = customersSearch;
        }

        public void Init()
        {
            // Build the index for the Customers search
            this._customersSearch.BuildIndex();
        }

        public void Search(string searchTerm, int numberOfResults = 9999)
        {
            // Search for customers
            var documents = this._customersSearch.DoSearch(searchTerm, numberOfResults);

            if (documents.Any())
            {
                Console.WriteLine($"Top {numberOfResults} results when searching for customers using the search term '{searchTerm}':");

                foreach (var _doc in documents)
                {
                    Console.WriteLine(_doc.Get("fullName"));
                }
            }
        }
    }
}
