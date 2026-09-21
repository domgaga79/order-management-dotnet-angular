using OrderManagement.Domain.Entities;
namespace OrderManagement.Application.Interfaces.Repositories;
public interface IProductRepository { Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct=default); Task<Product?> GetByIdAsync(Guid id,CancellationToken ct=default); Task AddAsync(Product product,CancellationToken ct=default); Task UpdateAsync(Product product,CancellationToken ct=default); Task DeleteAsync(Product product,CancellationToken ct=default); }
