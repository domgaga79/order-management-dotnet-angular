using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.DTOs.Orders;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Infrastructure.Data;
using OrderManagement.Infrastructure.Services;

namespace OrderManagement.Tests.Integration;

[Collection(PostgreSqlCollection.Name)]
public sealed class OrderServiceIntegrationTests
{
    private readonly PostgreSqlFixture _fixture;

    public OrderServiceIntegrationTests(PostgreSqlFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task CreateAsync_DecreasesStockAndPreservesPriceSnapshot()
    {
        await _fixture.ResetAsync();
        await using var db = _fixture.CreateDbContext();
        var (customer, product) = await SeedAsync(db, stock: 10, price: 19.90m);
        var service = new OrderService(db);

        var result = await service.CreateAsync(new CreateOrderRequest
        {
            CustomerId = customer.Id,
            Items = { new CreateOrderItemRequest { ProductId = product.Id, Quantity = 2 } }
        });

        Assert.Equal("Pending", result.Status);
        Assert.Equal(39.80m, result.Total);

        db.ChangeTracker.Clear();
        var persistedProduct = await db.Products.SingleAsync(x => x.Id == product.Id);
        var item = await db.OrderItems.SingleAsync();

        Assert.Equal(8, persistedProduct.Stock);
        Assert.Equal(19.90m, item.UnitPrice);
        Assert.Equal(39.80m, item.Subtotal);
    }

    [Fact]
    public async Task CreateAsync_GroupsRepeatedProductLines()
    {
        await _fixture.ResetAsync();
        await using var db = _fixture.CreateDbContext();
        var (customer, product) = await SeedAsync(db, stock: 10, price: 50m);
        var service = new OrderService(db);

        var result = await service.CreateAsync(new CreateOrderRequest
        {
            CustomerId = customer.Id,
            Items =
            {
                new CreateOrderItemRequest { ProductId = product.Id, Quantity = 1 },
                new CreateOrderItemRequest { ProductId = product.Id, Quantity = 2 }
            }
        });

        var item = Assert.Single(result.Items);
        Assert.Equal(3, item.Quantity);
        Assert.Equal(150m, result.Total);

        db.ChangeTracker.Clear();
        Assert.Equal(7, (await db.Products.SingleAsync(x => x.Id == product.Id)).Stock);
    }

    [Fact]
    public async Task CreateAsync_WhenStockIsInsufficient_RollsBackOrder()
    {
        await _fixture.ResetAsync();
        await using var db = _fixture.CreateDbContext();
        var (customer, product) = await SeedAsync(db, stock: 1, price: 100m);
        var service = new OrderService(db);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateOrderRequest
            {
                CustomerId = customer.Id,
                Items = { new CreateOrderItemRequest { ProductId = product.Id, Quantity = 2 } }
            }));

        Assert.Contains("Insufficient stock", ex.Message);

        db.ChangeTracker.Clear();
        Assert.Equal(1, (await db.Products.SingleAsync(x => x.Id == product.Id)).Stock);
        Assert.Equal(0, await db.Orders.CountAsync());
        Assert.Equal(0, await db.OrderItems.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_WhenCustomerIsInactive_RejectsOrder()
    {
        await _fixture.ResetAsync();
        await using var db = _fixture.CreateDbContext();
        var (customer, product) = await SeedAsync(db, customerActive: false);
        var service = new OrderService(db);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateOrderRequest
            {
                CustomerId = customer.Id,
                Items = { new CreateOrderItemRequest { ProductId = product.Id, Quantity = 1 } }
            }));

        Assert.Contains("Customer not found or inactive", ex.Message);
        Assert.Equal(0, await db.Orders.CountAsync());
    }

    [Fact]
    public async Task CancelAsync_RestoresStockOnlyOnce()
    {
        await _fixture.ResetAsync();
        await using var db = _fixture.CreateDbContext();
        var (customer, product) = await SeedAsync(db, stock: 10, price: 25m);
        var service = new OrderService(db);

        var created = await service.CreateAsync(new CreateOrderRequest
        {
            CustomerId = customer.Id,
            Items = { new CreateOrderItemRequest { ProductId = product.Id, Quantity = 4 } }
        });

        Assert.True(await service.CancelAsync(created.Id));
        Assert.True(await service.CancelAsync(created.Id));

        db.ChangeTracker.Clear();
        var persistedProduct = await db.Products.SingleAsync(x => x.Id == product.Id);
        var persistedOrder = await db.Orders.SingleAsync(x => x.Id == created.Id);

        Assert.Equal(10, persistedProduct.Stock);
        Assert.Equal(OrderStatus.Cancelled, persistedOrder.Status);
    }

    [Fact]
    public async Task CompleteAsync_PreventsLaterCancellation()
    {
        await _fixture.ResetAsync();
        await using var db = _fixture.CreateDbContext();
        var (customer, product) = await SeedAsync(db, stock: 5, price: 30m);
        var service = new OrderService(db);

        var created = await service.CreateAsync(new CreateOrderRequest
        {
            CustomerId = customer.Id,
            Items = { new CreateOrderItemRequest { ProductId = product.Id, Quantity = 2 } }
        });

        Assert.True(await service.CompleteAsync(created.Id));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CancelAsync(created.Id));

        Assert.Contains("Completed order cannot be cancelled", ex.Message);

        db.ChangeTracker.Clear();
        Assert.Equal(3, (await db.Products.SingleAsync(x => x.Id == product.Id)).Stock);
        Assert.Equal(OrderStatus.Completed, (await db.Orders.SingleAsync(x => x.Id == created.Id)).Status);
    }

    [Fact]
    public async Task CompleteAsync_WhenOrderWasCancelled_Throws()
    {
        await _fixture.ResetAsync();
        await using var db = _fixture.CreateDbContext();
        var (customer, product) = await SeedAsync(db, stock: 5, price: 30m);
        var service = new OrderService(db);

        var created = await service.CreateAsync(new CreateOrderRequest
        {
            CustomerId = customer.Id,
            Items = { new CreateOrderItemRequest { ProductId = product.Id, Quantity = 1 } }
        });

        Assert.True(await service.CancelAsync(created.Id));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CompleteAsync(created.Id));

        Assert.Contains("Cancelled order cannot be completed", ex.Message);
    }

    private static async Task<(Customer Customer, Product Product)> SeedAsync(
        AppDbContext db,
        int stock = 10,
        decimal price = 100m,
        bool customerActive = true,
        bool productActive = true)
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = "Cliente Teste",
            Email = "cliente@teste.local",
            Active = customerActive
        };

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Produto Teste",
            Price = price,
            Stock = stock,
            Active = productActive
        };

        db.AddRange(customer, product);
        await db.SaveChangesAsync();
        return (customer, product);
    }
}
