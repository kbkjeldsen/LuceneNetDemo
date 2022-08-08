using Bogus;

using LuceneDemo.WebApi.Models;

namespace LuceneDemo.WebApi.Data
{
    public static class DummyData
    {
        private static readonly int RANDOMIZER_SEED = 584256151;

        public static IEnumerable<ISimpleCustomer> GetDummyCustomers(int numberOfCustomers = 1)
        {

            Randomizer.Seed = new Random(RANDOMIZER_SEED);

            var totalCustomersGeneratedCounter = 1;

            var dummyCustomers = new Faker<Customer>()
                .RuleFor(c => c.CustomerKey, f => ConvertToCustomerKey(totalCustomersGeneratedCounter++))
                .RuleFor(c => c.FullName, (f, c) => f.Name.FullName());

            return dummyCustomers.Generate(numberOfCustomers);
        }

        private static string ConvertToCustomerKey(int number)
        {
            // Generate a number to build a gross string of at least 10 characters with leading zero's for the customer key
            var grossString = "000000000" + number.ToString();
            // Custoemr key is the last 10 cahracters of the gross string
            return grossString.Substring(grossString.Length - 10, 10);
        }
    }
}
