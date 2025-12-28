
using MaghsalatiSPlus.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MaghsalatiSPlus.Data
{
    public class AppDbContext : IdentityDbContext<ShopOwner>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Order>().Property(p => p.TotalAmount).HasColumnType("decimal(18,2)");
            builder.Entity<OrderItem>().Property(p => p.Price).HasColumnType("decimal(18,2)");

            builder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Category)
                .WithMany() 
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}