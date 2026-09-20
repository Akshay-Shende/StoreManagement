using StoreManagement.Application.DTOs;
namespace StoreManagement.Application.Interfaces;
public interface ICustomerService
{
    Task<IReadOnlyCollection<CustomerResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<CustomerResponse> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken);
}
