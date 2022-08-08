
using LuceneDemo.WebApi.Models;

using Microsoft.EntityFrameworkCore;

namespace Customers.Data
{
    public partial class CustomersDbContext : DbContext
    {
        public CustomersDbContext()
        {
        }

        public CustomersDbContext(DbContextOptions<CustomersDbContext> options)
            : base(options)
        {
        }
        public virtual DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.CustomerKey);

                entity.Property(e => e.CustomerKey).IsRequired();
                entity.Property(e => e.FullName).IsRequired();
            });
        }
    }
}
