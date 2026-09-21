using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.DTOs.Orders;
using OrderManagement.Application.Interfaces.Services;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Infrastructure.Data;

namespace OrderManagement.Infrastructure.Services;

public sealed class OrderService : IOrderService
{
    private readonly AppDbContext _db;
    public OrderService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<OrderResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var orders = await _db.Orders.AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(x => x.Product)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
        return orders.Select(Map).ToList();
    }

    public async Task<OrderResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var order = await _db.Orders.AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return order is null ? null : Map(order);
    }

    public async Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken ct = default)
    {
        if (request.Items.Count == 0)
            throw new InvalidOperationException("Order must contain at least one item.");

        await using var transaction = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var customer = await _db.Customers.FirstOrDefaultAsync(x => x.Id == request.CustomerId && x.Active, ct)
                ?? throw new InvalidOperationException("Customer not found or inactive.");

            var groupedItems = request.Items
                .GroupBy(x => x.ProductId)
                .Select(group => new { ProductId = group.Key, Quantity = group.Sum(x => x.Quantity) })
                .ToList();

            var productIds = groupedItems.Select(x => x.ProductId).ToArray();
            var products = await _db.Products.Where(x => productIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = customer.Id,
                Customer = customer,
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var item in groupedItems)
            {
                if (!products.TryGetValue(item.ProductId, out var product) || !product.Active)
                    throw new InvalidOperationException($"Product {item.ProductId} not found or inactive.");

                await _db.Database.ExecuteSqlInterpolatedAsync(
                    $"SELECT 1 FROM products WHERE \"Id\" = {product.Id} FOR UPDATE", ct);
                await _db.Entry(product).ReloadAsync(ct);

                if (product.Stock < item.Quantity)
                    throw new InvalidOperationException($"Insufficient stock for {product.Name}.");

                product.Stock -= item.Quantity;
                var subtotal = product.Price * item.Quantity;
                order.Items.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    Product = product,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                    Subtotal = subtotal
                });
                order.Total += subtotal;
            }

            _db.Orders.Add(order);
            await _db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return Map(order);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<bool> CompleteAsync(Guid id, CancellationToken ct = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(ct);
        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT 1 FROM orders WHERE \"Id\" = {id} FOR UPDATE", ct);
        var order = await _db.Orders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (order is null) return false;
        if (order.Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Cancelled order cannot be completed.");
        if (order.Status == OrderStatus.Completed) return true;

        order.Status = OrderStatus.Completed;
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return true;
    }

    public async Task<bool> CancelAsync(Guid id, CancellationToken ct = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(ct);
        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT 1 FROM orders WHERE \"Id\" = {id} FOR UPDATE", ct);
        var order = await _db.Orders
            .Include(x => x.Items).ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (order is null) return false;
        if (order.Status == OrderStatus.Cancelled) return true;
        if (order.Status == OrderStatus.Completed)
            throw new InvalidOperationException("Completed order cannot be cancelled.");

        foreach (var item in order.Items) item.Product.Stock += item.Quantity;
        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return true;
    }

    private static OrderResponse Map(Order order) => new(
        order.Id,
        order.CustomerId,
        order.Customer?.Name ?? string.Empty,
        order.Status.ToString(),
        order.Total,
        order.CreatedAt,
        order.Items.Select(i => new OrderItemResponse(
            i.ProductId,
            i.Product?.Name ?? string.Empty,
            i.Quantity,
            i.UnitPrice,
            i.Subtotal)).ToList());
}
