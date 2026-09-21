namespace OrderManagement.Domain.Entities;
public class Category {
 public Guid Id { get; set; }
 public string Name { get; set; } = string.Empty;
 public bool Active { get; set; } = true;
 public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
 public ICollection<Product> Products { get; set; } = new List<Product>();
}
