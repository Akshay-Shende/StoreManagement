namespace StoreManagement.Domain.Entities;

public class Batch
{
    public long BatchId { get; set; }
    public long ProductId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public decimal ReceivedQuantity { get; set; }
    public decimal AvailableQuantity { get; set; }
    public DateOnly? ManufacturingDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;
    public bool IsSellable { get; set; } = true;

    public Product Product { get; set; } = null!;
    public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
    public ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
}
