using StoreManagement.Application.DTOs;
namespace StoreManagement.Application.Interfaces;
public interface IReturnService
{
    Task<ReturnResponse> CreateAsync(CreateReturnRequest request, string createdBy, CancellationToken cancellationToken);
}
