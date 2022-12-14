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
// - Add the search engine service as singleton, since this will be a common, shared service to be provided for all consumers.
builder.Services.AddSingleton<ICustomersSearchEngine, CustomersSearchEngine>();
// Add the search background service as hosted (singleton) service.
// - As this service extends the 'BackgroundService' abstact class, the 'ExecuteAsync'
//   method within the service class will be executed when the application starts.
builder.Services.AddHostedService<SearchBackgroundService>();

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
