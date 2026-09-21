using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OrderManagement.Api.Services;
using OrderManagement.Application.DTOs.Auth;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Tests.Integration;

[Collection(PostgreSqlCollection.Name)]
public sealed class AuthServiceIntegrationTests
{
    private readonly PostgreSqlFixture _fixture;

    public AuthServiceIntegrationTests(PostgreSqlFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task RegisterAsync_FirstUserCanBeAdmin()
    {
        await _fixture.ResetAsync();
        await using var db = _fixture.CreateDbContext();
        var service = new AuthService(db, BuildConfiguration());

        var result = await service.RegisterAsync(new RegisterRequest
        {
            Name = " Administrador ",
            Email = " ADMIN@LOCAL.TEST ",
            Password = "Admin123!",
            Role = UserRole.Admin
        });

        Assert.Equal("Administrador", result.Name);
        Assert.Equal("admin@local.test", result.Email);
        Assert.Equal("Admin", result.Role);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
    }

    [Fact]
    public async Task RegisterAsync_SecondUserIsForcedToOperator()
    {
        await _fixture.ResetAsync();
        await using var db = _fixture.CreateDbContext();
        var service = new AuthService(db, BuildConfiguration());

        await service.RegisterAsync(new RegisterRequest
        {
            Name = "Primeiro",
            Email = "primeiro@local.test",
            Password = "Admin123!",
            Role = UserRole.Admin
        });

        var second = await service.RegisterAsync(new RegisterRequest
        {
            Name = "Segundo",
            Email = "segundo@local.test",
            Password = "Admin123!",
            Role = UserRole.Admin
        });

        Assert.Equal("Operator", second.Role);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmailThrows()
    {
        await _fixture.ResetAsync();
        await using var db = _fixture.CreateDbContext();
        var service = new AuthService(db, BuildConfiguration());

        await service.RegisterAsync(new RegisterRequest
        {
            Name = "Primeiro",
            Email = "usuario@local.test",
            Password = "Admin123!"
        });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RegisterAsync(new RegisterRequest
            {
                Name = "Duplicado",
                Email = " USUARIO@LOCAL.TEST ",
                Password = "Admin123!"
            }));

        Assert.Contains("Email already registered", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsJwt()
    {
        await _fixture.ResetAsync();
        await using var db = _fixture.CreateDbContext();
        var service = new AuthService(db, BuildConfiguration());

        await service.RegisterAsync(new RegisterRequest
        {
            Name = "Administrador",
            Email = "admin@local.test",
            Password = "Admin123!",
            Role = UserRole.Admin
        });

        var result = await service.LoginAsync(new LoginRequest
        {
            Email = " ADMIN@LOCAL.TEST ",
            Password = "Admin123!"
        });

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result!.Token));
        Assert.Equal("Admin", result.Role);
        Assert.Equal("admin@local.test", result.Email);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ReturnsNull()
    {
        await _fixture.ResetAsync();
        await using var db = _fixture.CreateDbContext();
        var service = new AuthService(db, BuildConfiguration());

        await service.RegisterAsync(new RegisterRequest
        {
            Name = "Administrador",
            Email = "admin@local.test",
            Password = "Admin123!",
            Role = UserRole.Admin
        });

        var result = await service.LoginAsync(new LoginRequest
        {
            Email = "admin@local.test",
            Password = "senha-errada"
        });

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_WhenUserIsInactive_ReturnsNull()
    {
        await _fixture.ResetAsync();
        await using var db = _fixture.CreateDbContext();
        var service = new AuthService(db, BuildConfiguration());

        await service.RegisterAsync(new RegisterRequest
        {
            Name = "Administrador",
            Email = "admin@local.test",
            Password = "Admin123!",
            Role = UserRole.Admin
        });

        var user = await db.Users.SingleAsync();
        user.Active = false;
        await db.SaveChangesAsync();

        var result = await service.LoginAsync(new LoginRequest
        {
            Email = "admin@local.test",
            Password = "Admin123!"
        });

        Assert.Null(result);
    }

    private static IConfiguration BuildConfiguration() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-key-with-more-than-thirty-two-characters-123456789",
                ["Jwt:Issuer"] = "OrderManagement.Tests"
            })
            .Build();
}
