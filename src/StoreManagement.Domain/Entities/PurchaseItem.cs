namespace StoreManagement.Domain.Entities;

public class PurchaseItem
{
    public Guid PurchaseItemId { get; set; } = Guid.NewGuid();
    public Guid PurchaseId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public Guid? BatchId { get; set; }

    public Purchase Purchase { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public Batch? Batch { get; set; }
}
