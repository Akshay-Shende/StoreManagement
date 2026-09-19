using StoreManagement.Domain.Enums;

namespace StoreManagement.Domain.Entities;

public class InventoryTransaction
{
    public Guid TransactionId { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public Guid? BatchId { get; set; }
    public InventoryTransactionType TransactionType { get; set; }
    public decimal QuantityDelta { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? Reason { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string CreatedBy { get; set; } = "system";

    public Product Product { get; set; } = null!;
    public Batch? Batch { get; set; }
}
