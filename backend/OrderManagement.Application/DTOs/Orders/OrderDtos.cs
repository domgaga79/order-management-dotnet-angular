using System.ComponentModel.DataAnnotations;
namespace OrderManagement.Application.DTOs.Orders;
public sealed class CreateOrderItemRequest { public Guid ProductId {get;set;} [Range(1,int.MaxValue)] public int Quantity {get;set;} }
public sealed class CreateOrderRequest { public Guid CustomerId {get;set;} [MinLength(1)] public List<CreateOrderItemRequest> Items {get;set;}=new(); }
public sealed record OrderItemResponse(Guid ProductId,string ProductName,int Quantity,decimal UnitPrice,decimal Subtotal);
public sealed record OrderResponse(Guid Id,Guid CustomerId,string CustomerName,string Status,decimal Total,DateTime CreatedAt,IReadOnlyList<OrderItemResponse> Items);
