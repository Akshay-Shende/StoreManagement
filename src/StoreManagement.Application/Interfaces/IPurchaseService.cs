using StoreManagement.Application.DTOs;

namespace StoreManagement.Application.Interfaces;

public interface IPurchaseService
{
    Task<PurchaseResponse> CreateAsync(CreatePurchaseRequest request, CancellationToken cancellationToken);
    Task<PurchaseResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<PurchaseResponse?> ReceiveAsync(Guid id, ReceivePurchaseRequest request, string createdBy, CancellationToken cancellationToken);
}
