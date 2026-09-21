using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.DTOs.Categories; using OrderManagement.Application.Interfaces.Services; using OrderManagement.Domain.Entities; using OrderManagement.Infrastructure.Data;
namespace OrderManagement.Infrastructure.Services;
public sealed class CategoryService:ICategoryService {private readonly AppDbContext _db; public CategoryService(AppDbContext db)=>_db=db;
 public async Task<IReadOnlyList<CategoryResponse>> GetAllAsync(CancellationToken ct=default)=>await _db.Categories.AsNoTracking().OrderBy(x=>x.Name).Select(x=>new CategoryResponse(x.Id,x.Name,x.Active,x.CreatedAt)).ToListAsync(ct);
 public async Task<CategoryResponse?> GetByIdAsync(Guid id,CancellationToken ct=default)=>await _db.Categories.AsNoTracking().Where(x=>x.Id==id).Select(x=>new CategoryResponse(x.Id,x.Name,x.Active,x.CreatedAt)).FirstOrDefaultAsync(ct);
 public async Task<CategoryResponse> CreateAsync(CategoryRequest r,CancellationToken ct=default){var x=new Category{Id=Guid.NewGuid(),Name=r.Name.Trim(),Active=r.Active,CreatedAt=DateTime.UtcNow};_db.Categories.Add(x);await _db.SaveChangesAsync(ct);return new(x.Id,x.Name,x.Active,x.CreatedAt);} 
 public async Task<CategoryResponse?> UpdateAsync(Guid id,CategoryRequest r,CancellationToken ct=default){var x=await _db.Categories.FindAsync(new object[] { id },ct);if(x is null)return null;x.Name=r.Name.Trim();x.Active=r.Active;await _db.SaveChangesAsync(ct);return new(x.Id,x.Name,x.Active,x.CreatedAt);} 
 public async Task<bool> DeleteAsync(Guid id,CancellationToken ct=default){var x=await _db.Categories.FindAsync(new object[] { id },ct);if(x is null)return false;_db.Categories.Remove(x);await _db.SaveChangesAsync(ct);return true;}}
