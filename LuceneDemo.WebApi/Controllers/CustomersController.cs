using LuceneDemo.WebApi.Models;
using LuceneDemo.WebApi.Search;

using Microsoft.AspNetCore.Mvc;

namespace LuceneDemo.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ILogger<CustomersController> _logger;
    private readonly ICustomersSearchService _customersSearch;

    public CustomersController(ILogger<CustomersController> logger, ICustomersSearchService customersSearch)
    {
        this._logger = logger;
        this._customersSearch = customersSearch;
    }

    [HttpGet]
    [Route("search", Name = "SearchForCustomers")]
    public ActionResult<IEnumerable<ISimpleCustomer>> Get(string q, int maxresults = 100)
    {
        // Search for customer documents
        var documents = this._customersSearch.DoSearch(q, maxresults);

        var customers = documents.Select(doc => new CustomerDto(doc.Get("customerKey"), doc.Get("fullName")));

        return customers.ToList();
    }
}
