﻿using Customers.Data;

using EFCore.BulkExtensions;

using LuceneDemo.WebApi.Data;
using LuceneDemo.WebApi.Models;
using LuceneDemo.WebApi.Search;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LuceneDemo.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ILogger<CustomersController> _logger;
    private readonly ICustomersSearchService _customersSearch;
    private readonly CustomersDbContext _customersDbContext;

    public CustomersController(ILogger<CustomersController> logger, ICustomersSearchService customersSearch, CustomersDbContext customersDbContext)
    {
        this._logger = logger;
        this._customersSearch = customersSearch;
        this._customersDbContext = customersDbContext;
    }

    /// <summary>
    /// Search for customers.
    /// </summary>
    /// <param name="q">The term to search for.</param>
    /// <param name="maxresults">The maximum number of results to return.</param>
    /// <returns></returns>
    [HttpGet]
    [Route("search", Name = "SearchForCustomers")]
    public ActionResult<IEnumerable<ISimpleCustomer>> SearchForCustomers(string q, int maxresults = 100)
    {
        // Search for customers 
        var customers = this._customersSearch.SearchForCustomers(q, maxresults);

        return customers.ToList();
    }

    /// <summary>
    /// Get all customers.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ISimpleCustomer>>> GetCustomers()
    {
        var numOfCustToReturn = 1000;

        var allCustomers = this._customersDbContext.Customers;

        Console.WriteLine($"Returning TOP {numOfCustToReturn} of {allCustomers.Count()} customers from database");

        var customersToReturn = await allCustomers.Take(numOfCustToReturn).ToListAsync();

        return customersToReturn;
    }

    [HttpPost]
    public async Task<ActionResult<IEnumerable<ISimpleCustomer>>> ReplaceDummyData([FromBody] int numberOfCustomers)
    {
        // Truncate Customers table
        await this._customersDbContext.TruncateAsync<Customer>();

        var customersToInsert = DummyData.GetDummyCustomers(numberOfCustomers);

        // Insert new dummy customers
        await this._customersDbContext.BulkInsertAsync(customersToInsert.ToList());

        // Initiate a rebuild of the search index
        await this._customersSearch.RefreshCustomersSearchIndexAsync();

        return await this.GetCustomers();
    }
}
