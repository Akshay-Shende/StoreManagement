namespace StoreManagement.Domain.Entities;

public class PurchaseItem
{
    public long PurchaseItemId { get; set; }
    public long PurchaseId { get; set; }
    public long ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public long? BatchId { get; set; }

    public Purchase Purchase { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public Batch? Batch { get; set; }
}
