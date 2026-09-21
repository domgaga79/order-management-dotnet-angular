using OrderManagement.Application.DTOs.Customers;
namespace OrderManagement.Application.Interfaces.Services;
public interface ICustomerService { Task<IReadOnlyList<CustomerResponse>> GetAllAsync(CancellationToken ct=default); Task<CustomerResponse?> GetByIdAsync(Guid id,CancellationToken ct=default); Task<CustomerResponse> CreateAsync(CustomerRequest request,CancellationToken ct=default); Task<CustomerResponse?> UpdateAsync(Guid id,CustomerRequest request,CancellationToken ct=default); Task<bool> DeleteAsync(Guid id,CancellationToken ct=default); }
