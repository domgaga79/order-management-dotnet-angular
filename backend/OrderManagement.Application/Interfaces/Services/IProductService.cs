using OrderManagement.Application.DTOs.Products;
namespace OrderManagement.Application.Interfaces.Services;
public interface IProductService { Task<IReadOnlyList<ProductResponse>> GetAllAsync(CancellationToken ct=default); Task<ProductResponse?> GetByIdAsync(Guid id,CancellationToken ct=default); Task<ProductResponse> CreateAsync(CreateProductRequest request,CancellationToken ct=default); Task<ProductResponse?> UpdateAsync(Guid id,UpdateProductRequest request,CancellationToken ct=default); Task<bool> DeleteAsync(Guid id,CancellationToken ct=default); }
