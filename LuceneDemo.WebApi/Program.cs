using Customers.Data;

using EFCore.BulkExtensions;

using LuceneDemo.WebApi;
using LuceneDemo.WebApi.Data;
using LuceneDemo.WebApi.Search;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// SQL Lite connection
// Since this is a demo app, we use an in memory database that is recreated when the app starts.
var sqLiteConnStrBuilder = new SqliteConnectionStringBuilder()
{
    DataSource = "Customers",
    Mode = SqliteOpenMode.Memory,
    Cache = SqliteCacheMode.Shared
};
var sqlLiteInMemoryConnection = new SqliteConnection(sqLiteConnStrBuilder.ConnectionString);
sqlLiteInMemoryConnection.Open();

// ----------------------------------------------
// Add services to the container.
// ----------------------------------------------

// Databases
//builder.Services.AddDbContext<CustomersDbContext>(options => options.UseInMemoryDatabase("Customers"));
builder.Services.AddDbContext<CustomersDbContext>(options => options.UseSqlite(sqlLiteInMemoryConnection));

// SearchEngine
// - Add as singletion, since this service will be a common, shared service to be provided for all 
builder.Services.AddSingleton<ICustomersSearchService, CustomersSearchService>();

// Controllers
builder.Services.AddControllers();

// Swagger
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ----------------------------------------------------------------------------
// Create database and seed dummy data in Development
// ----------------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    // Create database
    var customersDbContext = app.Services.CreateScope().ServiceProvider.GetService<CustomersDbContext>();
    customersDbContext.Database.EnsureCreated();

    // Insert dummy data
    var numberOfDummyCustomersToGenerate = 100000;
    customersDbContext.BulkInsert(DummyData.GetDummyCustomers(numberOfDummyCustomersToGenerate).ToList());
}

// ----------------------------------------------------------------------------
// Initialize the search engine
// ----------------------------------------------------------------------------

var searchEngine = new SearchEngine(app.Services.GetRequiredService<ICustomersSearchService>());
searchEngine.Init();

// Perform a test search in development environment
if (app.Environment.IsDevelopment())
{
    searchEngine.Search("wal", 50);
}

// ----------------------------------------------------------------------------
// Configure the HTTP request pipeline.
// ----------------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// ----------------------------------------------------------------------------
// Run the app.
// ----------------------------------------------------------------------------

app.Run();
