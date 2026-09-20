using StoreManagement.Application.DTOs;

namespace StoreManagement.Application.Interfaces;

public interface IPurchaseService
{
    Task<PurchaseResponse> CreateAsync(CreatePurchaseRequest request, CancellationToken cancellationToken);
    Task<PurchaseResponse?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<PurchaseResponse?> ReceiveAsync(long id, ReceivePurchaseRequest request, string createdBy, CancellationToken cancellationToken);
}
