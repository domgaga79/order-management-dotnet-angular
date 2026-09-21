using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Interfaces.Repositories;
using OrderManagement.Domain.Entities;
using OrderManagement.Infrastructure.Data;
namespace OrderManagement.Infrastructure.Repositories;
public sealed class ProductRepository:IProductRepository { private readonly AppDbContext _db; public ProductRepository(AppDbContext db)=>_db=db;
 public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct=default)=>await _db.Products.AsNoTracking().Include(x=>x.Category).OrderBy(x=>x.Name).ToListAsync(ct);
 public Task<Product?> GetByIdAsync(Guid id,CancellationToken ct=default)=>_db.Products.Include(x=>x.Category).FirstOrDefaultAsync(x=>x.Id==id,ct);
 public async Task AddAsync(Product p,CancellationToken ct=default){await _db.Products.AddAsync(p,ct);await _db.SaveChangesAsync(ct); if(p.CategoryId.HasValue) await _db.Entry(p).Reference(x=>x.Category).LoadAsync(ct);} 
 public async Task UpdateAsync(Product p,CancellationToken ct=default){await _db.SaveChangesAsync(ct); if(p.CategoryId.HasValue) await _db.Entry(p).Reference(x=>x.Category).LoadAsync(ct);} 
 public async Task DeleteAsync(Product p,CancellationToken ct=default){_db.Products.Remove(p);await _db.SaveChangesAsync(ct);} }
