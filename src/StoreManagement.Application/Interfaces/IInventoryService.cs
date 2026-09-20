using StoreManagement.Application.DTOs;
using StoreManagement.Domain.Enums;

namespace StoreManagement.Application.Interfaces;

public interface IInventoryService
{
    Task<IReadOnlyCollection<InventoryResponse>> GetCurrentAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<InventoryResponse>> GetLowStockAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ExpiringBatchResponse>> GetExpiringAsync(int days, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ExpiringBatchResponse>> GetExpiredAsync(CancellationToken cancellationToken);
    Task<StockAdjustmentResponse> CreateAdjustmentAsync(CreateStockAdjustmentRequest request, CancellationToken cancellationToken);
    Task<StockAdjustmentResponse?> ApproveAdjustmentAsync(long id, ApproveStockAdjustmentRequest request, string createdBy, CancellationToken cancellationToken);
    Task<ExpiringBatchResponse?> ExpireBatchAsync(long batchId, string createdBy, CancellationToken cancellationToken);
    Task<InventoryResponse?> RecordMovementAsync(InventoryTransactionType movementType, InventoryMovementRequest request, string createdBy, CancellationToken cancellationToken);
}
