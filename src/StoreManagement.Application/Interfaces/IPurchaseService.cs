using StoreManagement.Application.DTOs;
namespace StoreManagement.Application.Interfaces;
public interface IPurchaseService
{
    Task<IReadOnlyCollection<PurchaseResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<PurchaseResponse> CreateAsync(CreatePurchaseRequest request, string? clientRequestId, CancellationToken cancellationToken);
    Task<PurchaseResponse?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<PurchaseResponse?> ReceiveAsync(long id, ReceivePurchaseRequest request, string createdBy, string? clientRequestId, CancellationToken cancellationToken);
    Task<PurchaseResponse?> CancelAsync(long id, string actor, CancellationToken cancellationToken);
}
