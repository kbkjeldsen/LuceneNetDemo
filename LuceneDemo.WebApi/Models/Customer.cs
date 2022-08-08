namespace LuceneDemo.WebApi.Models
{
    public class Customer : ISimpleCustomer
    {
        public string CustomerKey { get; set; }
        public string FullName { get; set; }
    }
}
