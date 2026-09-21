using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;
namespace OrderManagement.Infrastructure.Data;
public class AppDbContext:DbContext {
 public AppDbContext(DbContextOptions<AppDbContext> options):base(options){}
 public DbSet<User> Users=>Set<User>(); public DbSet<Category> Categories=>Set<Category>(); public DbSet<Product> Products=>Set<Product>(); public DbSet<Customer> Customers=>Set<Customer>(); public DbSet<Order> Orders=>Set<Order>(); public DbSet<OrderItem> OrderItems=>Set<OrderItem>();
 protected override void OnModelCreating(ModelBuilder b){base.OnModelCreating(b);
  b.Entity<User>(e=>{e.ToTable("users");e.HasKey(x=>x.Id);e.HasIndex(x=>x.Email).IsUnique();e.Property(x=>x.Name).HasMaxLength(120).IsRequired();e.Property(x=>x.Email).HasMaxLength(180).IsRequired();e.Property(x=>x.PasswordHash).HasMaxLength(255).IsRequired();e.Property(x=>x.Role).HasConversion<string>().HasMaxLength(30);});
  b.Entity<Category>(e=>{e.ToTable("categories");e.HasKey(x=>x.Id);e.Property(x=>x.Name).HasMaxLength(120).IsRequired();});
  b.Entity<Product>(e=>{e.ToTable("products");e.HasKey(x=>x.Id);e.Property(x=>x.Name).HasMaxLength(150).IsRequired();e.Property(x=>x.Description).HasMaxLength(500);e.Property(x=>x.Price).HasPrecision(12,2);e.HasOne(x=>x.Category).WithMany(x=>x.Products).HasForeignKey(x=>x.CategoryId).OnDelete(DeleteBehavior.SetNull);});
  b.Entity<Customer>(e=>{e.ToTable("customers");e.HasKey(x=>x.Id);e.Property(x=>x.Name).HasMaxLength(150).IsRequired();e.Property(x=>x.Email).HasMaxLength(180);e.Property(x=>x.Phone).HasMaxLength(30);});
  b.Entity<Order>(e=>{e.ToTable("orders");e.HasKey(x=>x.Id);e.Property(x=>x.Status).HasConversion<string>().HasMaxLength(30);e.Property(x=>x.Total).HasPrecision(12,2);e.HasOne(x=>x.Customer).WithMany(x=>x.Orders).HasForeignKey(x=>x.CustomerId).OnDelete(DeleteBehavior.Restrict);});
  b.Entity<OrderItem>(e=>{e.ToTable("order_items");e.HasKey(x=>x.Id);e.Property(x=>x.UnitPrice).HasPrecision(12,2);e.Property(x=>x.Subtotal).HasPrecision(12,2);e.HasOne(x=>x.Order).WithMany(x=>x.Items).HasForeignKey(x=>x.OrderId).OnDelete(DeleteBehavior.Cascade);e.HasOne(x=>x.Product).WithMany().HasForeignKey(x=>x.ProductId).OnDelete(DeleteBehavior.Restrict);});
 }
}
