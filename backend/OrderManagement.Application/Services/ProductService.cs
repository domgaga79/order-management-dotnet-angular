using OrderManagement.Application.DTOs.Products;
using OrderManagement.Application.Interfaces.Repositories;
using OrderManagement.Application.Interfaces.Services;
using OrderManagement.Domain.Entities;
namespace OrderManagement.Application.Services;
public sealed class ProductService : IProductService {
 private readonly IProductRepository _repo; public ProductService(IProductRepository repo)=>_repo=repo;
 public async Task<IReadOnlyList<ProductResponse>> GetAllAsync(CancellationToken ct=default)=>(await _repo.GetAllAsync(ct)).Select(Map).ToList();
 public async Task<ProductResponse?> GetByIdAsync(Guid id,CancellationToken ct=default){var p=await _repo.GetByIdAsync(id,ct); return p is null?null:Map(p);} 
 public async Task<ProductResponse> CreateAsync(CreateProductRequest r,CancellationToken ct=default){var p=new Product{Id=Guid.NewGuid(),Name=r.Name.Trim(),Description=string.IsNullOrWhiteSpace(r.Description)?null:r.Description.Trim(),Price=r.Price,Stock=r.Stock,Active=r.Active,CategoryId=r.CategoryId,CreatedAt=DateTime.UtcNow}; await _repo.AddAsync(p,ct); return Map(p);} 
 public async Task<ProductResponse?> UpdateAsync(Guid id,UpdateProductRequest r,CancellationToken ct=default){var p=await _repo.GetByIdAsync(id,ct); if(p is null)return null; p.Name=r.Name.Trim();p.Description=string.IsNullOrWhiteSpace(r.Description)?null:r.Description.Trim();p.Price=r.Price;p.Stock=r.Stock;p.Active=r.Active;p.CategoryId=r.CategoryId;p.UpdatedAt=DateTime.UtcNow; await _repo.UpdateAsync(p,ct); return Map(p);} 
 public async Task<bool> DeleteAsync(Guid id,CancellationToken ct=default){var p=await _repo.GetByIdAsync(id,ct); if(p is null)return false; await _repo.DeleteAsync(p,ct); return true;} 
 private static ProductResponse Map(Product p)=>new(p.Id,p.Name,p.Description,p.Price,p.Stock,p.Active,p.CategoryId,p.Category?.Name,p.CreatedAt,p.UpdatedAt);
}
