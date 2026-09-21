using OrderManagement.Application.DTOs.Products;
using OrderManagement.Application.Interfaces.Repositories;
using OrderManagement.Application.Services;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Tests;

public sealed class ProductServiceTests
{
    [Fact]
    public async Task CreateAsync_TrimsFieldsAndPersistsProduct()
    {
        var repo = new FakeRepo();
        var service = new ProductService(repo);

        var result = await service.CreateAsync(new CreateProductRequest
        {
            Name = " Notebook ",
            Description = " Corporativo ",
            Price = 100m,
            Stock = 2
        });

        Assert.Equal("Notebook", result.Name);
        Assert.Equal("Corporativo", result.Description);
        Assert.Equal(100m, result.Price);
        Assert.Equal(2, result.Stock);
        Assert.Single(repo.Items);
    }

    [Fact]
    public async Task GetAllAsync_MapsCategoryData()
    {
        var categoryId = Guid.NewGuid();
        var repo = new FakeRepo();
        repo.Items.Add(new Product
        {
            Id = Guid.NewGuid(),
            Name = "Monitor",
            Price = 900m,
            Stock = 3,
            CategoryId = categoryId,
            Category = new Category { Id = categoryId, Name = "Periféricos" }
        });

        var service = new ProductService(repo);
        var result = await service.GetAllAsync();

        var item = Assert.Single(result);
        Assert.Equal("Monitor", item.Name);
        Assert.Equal(categoryId, item.CategoryId);
        Assert.Equal("Periféricos", item.CategoryName);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ReturnsNull()
    {
        var service = new ProductService(new FakeRepo());

        var result = await service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WhenFound_UpdatesFieldsAndTimestamp()
    {
        var id = Guid.NewGuid();
        var repo = new FakeRepo();
        repo.Items.Add(new Product
        {
            Id = id,
            Name = "Antes",
            Price = 10m,
            Stock = 1
        });

        var service = new ProductService(repo);

        var result = await service.UpdateAsync(id, new UpdateProductRequest
        {
            Name = " Depois ",
            Description = " Atualizado ",
            Price = 20m,
            Stock = 5,
            Active = false
        });

        Assert.NotNull(result);
        Assert.Equal("Depois", result!.Name);
        Assert.Equal("Atualizado", result.Description);
        Assert.Equal(20m, result.Price);
        Assert.Equal(5, result.Stock);
        Assert.False(result.Active);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_WhenMissing_ReturnsNull()
    {
        var service = new ProductService(new FakeRepo());

        var result = await service.UpdateAsync(Guid.NewGuid(), new UpdateProductRequest
        {
            Name = "Produto",
            Price = 10m,
            Stock = 1,
            Active = true
        });

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_WhenFound_RemovesProduct()
    {
        var id = Guid.NewGuid();
        var repo = new FakeRepo();
        repo.Items.Add(new Product { Id = id, Name = "Produto", Price = 10m, Stock = 1 });

        var service = new ProductService(repo);
        var result = await service.DeleteAsync(id);

        Assert.True(result);
        Assert.Empty(repo.Items);
    }

    [Fact]
    public async Task DeleteAsync_WhenMissing_ReturnsFalse()
    {
        var service = new ProductService(new FakeRepo());

        var result = await service.DeleteAsync(Guid.NewGuid());

        Assert.False(result);
    }

    private sealed class FakeRepo : IProductRepository
    {
        public List<Product> Items { get; } = new();

        public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<Product>>(Items.ToList());

        public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            Task.FromResult(Items.SingleOrDefault(x => x.Id == id));

        public Task AddAsync(Product product, CancellationToken ct = default)
        {
            Items.Add(product);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Product product, CancellationToken ct = default) =>
            Task.CompletedTask;

        public Task DeleteAsync(Product product, CancellationToken ct = default)
        {
            Items.Remove(product);
            return Task.CompletedTask;
        }
    }
}
