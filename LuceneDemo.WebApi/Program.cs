using System.Reflection;

using Customers.Data;

using EFCore.BulkExtensions;

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

// Databases //
//builder.Services.AddDbContext<CustomersDbContext>(options => options.UseInMemoryDatabase("Customers"));
builder.Services.AddDbContext<CustomersDbContext>(options => options.UseSqlite(sqlLiteInMemoryConnection));

// Customers Search //
builder.Services.AddScoped<ICustomersSearchService, CustomersSearchService>();
// - Add as singleton, since this service will be a common, shared service to be provided for all consumers.
builder.Services.AddSingleton<ICustomersSearchEngine, CustomersSearchEngine>();

// Controllers //
builder.Services.AddControllers();

// Swagger //
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

// ----------------------------------------------
// Build the app.
// ----------------------------------------------
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
    var numberOfDummyCustomersToGenerate = 10;
    customersDbContext.BulkInsert(DummyData.GetDummyCustomers(numberOfDummyCustomersToGenerate).ToList());
}

// ----------------------------------------------------------------------------
// Initialize the customer search engine
// ----------------------------------------------------------------------------
var customersSearch = app.Services.GetService<ICustomersSearchEngine>();
// Run the task that builds the customer index async so that the web app can continue startup.
Task.Run(() => customersSearch.RebuildIndexAsync());

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
