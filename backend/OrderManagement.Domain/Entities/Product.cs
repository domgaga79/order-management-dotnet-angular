namespace OrderManagement.Domain.Entities;
public class Product {
 public Guid Id { get; set; }
 public string Name { get; set; } = string.Empty;
 public string? Description { get; set; }
 public decimal Price { get; set; }
 public int Stock { get; set; }
 public bool Active { get; set; } = true;
 public Guid? CategoryId { get; set; }
 public Category? Category { get; set; }
 public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
 public DateTime? UpdatedAt { get; set; }
}
