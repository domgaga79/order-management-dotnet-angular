using OrderManagement.Domain.Enums;
namespace OrderManagement.Domain.Entities;
public class Order {
 public Guid Id { get; set; }
 public Guid CustomerId { get; set; }
 public Customer Customer { get; set; } = null!;
 public OrderStatus Status { get; set; } = OrderStatus.Pending;
 public decimal Total { get; set; }
 public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
 public DateTime? UpdatedAt { get; set; }
 public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
