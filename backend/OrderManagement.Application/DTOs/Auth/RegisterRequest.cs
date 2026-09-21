using System.ComponentModel.DataAnnotations;
using OrderManagement.Domain.Enums;
namespace OrderManagement.Application.DTOs.Auth;
public sealed class RegisterRequest { [Required, StringLength(120, MinimumLength=2)] public string Name { get; set; }=string.Empty; [Required, EmailAddress] public string Email {get;set;}=string.Empty; [Required, MinLength(8)] public string Password {get;set;}=string.Empty; public UserRole Role {get;set;}=UserRole.Operator; }
