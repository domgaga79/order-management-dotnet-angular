namespace OrderManagement.Domain.Entities;
public class Customer {
 public Guid Id { get; set; }
 public string Name { get; set; } = string.Empty;
 public string? Email { get; set; }
 public string? Phone { get; set; }
 public bool Active { get; set; } = true;
 public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
 public ICollection<Order> Orders { get; set; } = new List<Order>();
}
