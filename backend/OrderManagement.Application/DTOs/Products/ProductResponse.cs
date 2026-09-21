namespace OrderManagement.Application.DTOs.Products;
public sealed record ProductResponse(Guid Id,string Name,string? Description,decimal Price,int Stock,bool Active,Guid? CategoryId,string? CategoryName,DateTime CreatedAt,DateTime? UpdatedAt);
