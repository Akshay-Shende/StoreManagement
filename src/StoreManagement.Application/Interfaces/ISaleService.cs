using StoreManagement.Application.DTOs;
namespace StoreManagement.Application.Interfaces;
public interface ISaleService
{
    Task<SaleResponse> CreateAndCompleteAsync(CreateSaleRequest request, string createdBy, string? clientRequestId, CancellationToken cancellationToken);
    Task<SaleResponse> CreateInternalAsync(CreateSaleRequest request, string createdBy, string? clientRequestId, bool allowPriceOverride, CancellationToken cancellationToken);
    Task<SaleResponse?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<PaymentResponse?> AddPaymentAsync(long saleId, CreatePaymentRequest request, string createdBy, CancellationToken cancellationToken);
    Task<SaleResponse?> CancelAsync(long saleId, string createdBy, CancellationToken cancellationToken);
}
