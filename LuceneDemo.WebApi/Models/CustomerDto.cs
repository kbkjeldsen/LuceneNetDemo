namespace LuceneDemo.WebApi.Models
{
    public class CustomerDto : ISimpleCustomer
    {
        public CustomerDto()
        {
        }

        public CustomerDto(ISimpleCustomer customer)
        {
            this.CustomerKey = customer.CustomerKey;
            this.FullName = customer.FullName;
        }

        public CustomerDto(string customerKey, string fullName)
        {
            this.CustomerKey = customerKey;
            this.FullName = fullName;
        }

        public string CustomerKey { get; set; }
        public string FullName { get; set; }
    }
}
