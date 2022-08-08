using Customers.Data;

using LuceneDemo.WebApi.Models;

using Microsoft.AspNetCore.Mvc;

namespace LuceneDemo.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ILogger<CustomersController> _logger;
    private readonly CustomersDbContext _customersDbContext;

    public CustomersController(ILogger<CustomersController> logger, CustomersDbContext customersDbContext)
    {
        this._logger = logger;
        this._customersDbContext = customersDbContext;
    }

    [HttpGet(Name = "GetCustomers")]
    public IEnumerable<ISimpleCustomer> Get()
    {
        var customers = this._customersDbContext.Customers.Take(10);
        return customers.ToList();
    }
}
