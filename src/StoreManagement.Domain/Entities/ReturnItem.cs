namespace StoreManagement.Domain.Entities;

public class ReturnItem
{
    public long ReturnItemId { get; set; }
    public long ReturnId { get; set; }
    public long SaleItemId { get; set; }
    public long? BatchId { get; set; }
    public decimal Quantity { get; set; }
    public decimal RefundAmount { get; set; }

    public Return Return { get; set; } = null!;
    public SaleItem SaleItem { get; set; } = null!;
    public Batch? Batch { get; set; }
}
