using OrderManagement.Application.DTOs.Categories;
namespace OrderManagement.Application.Interfaces.Services;
public interface ICategoryService { Task<IReadOnlyList<CategoryResponse>> GetAllAsync(CancellationToken ct=default); Task<CategoryResponse?> GetByIdAsync(Guid id,CancellationToken ct=default); Task<CategoryResponse> CreateAsync(CategoryRequest request,CancellationToken ct=default); Task<CategoryResponse?> UpdateAsync(Guid id,CategoryRequest request,CancellationToken ct=default); Task<bool> DeleteAsync(Guid id,CancellationToken ct=default); }
