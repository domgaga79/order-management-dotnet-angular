using System.ComponentModel.DataAnnotations;
namespace OrderManagement.Application.DTOs.Products;
public sealed class CreateProductRequest {
 [Required, StringLength(150, MinimumLength=2)] public string Name {get;set;}=string.Empty;
 [StringLength(500)] public string? Description {get;set;}
 [Range(0.01, 9999999999.99)] public decimal Price {get;set;}
 [Range(0,int.MaxValue)] public int Stock {get;set;}
 public bool Active {get;set;}=true;
 public Guid? CategoryId {get;set;}
}
