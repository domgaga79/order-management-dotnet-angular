namespace OrderManagement.Application.DTOs.Auth;
public sealed record AuthResponse(string Token, DateTime ExpiresAt, Guid UserId, string Name, string Email, string Role);
