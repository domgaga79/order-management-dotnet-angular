using System.ComponentModel.DataAnnotations;
namespace OrderManagement.Application.DTOs.Categories;
public sealed class CategoryRequest { [Required, StringLength(120, MinimumLength=2)] public string Name {get;set;}=string.Empty; public bool Active {get;set;}=true; }
public sealed record CategoryResponse(Guid Id,string Name,bool Active,DateTime CreatedAt);
