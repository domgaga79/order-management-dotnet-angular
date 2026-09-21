using System.ComponentModel.DataAnnotations;
namespace OrderManagement.Application.DTOs.Customers;
public sealed class CustomerRequest { [Required, StringLength(150, MinimumLength=2)] public string Name {get;set;}=string.Empty; [EmailAddress] public string? Email {get;set;} [StringLength(30)] public string? Phone {get;set;} public bool Active {get;set;}=true; }
public sealed record CustomerResponse(Guid Id,string Name,string? Email,string? Phone,bool Active,DateTime CreatedAt);
