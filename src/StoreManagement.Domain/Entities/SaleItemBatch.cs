namespace StoreManagement.Domain.Entities;

public class SaleItemBatch
{
    public long SaleItemBatchId { get; set; }
    public long SaleItemId { get; set; }
    public long BatchId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }

    public SaleItem SaleItem { get; set; } = null!;
    public Batch Batch { get; set; } = null!;
}
