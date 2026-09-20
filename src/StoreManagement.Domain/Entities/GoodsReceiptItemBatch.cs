namespace StoreManagement.Domain.Entities;

public class GoodsReceiptItemBatch
{
    public long GoodsReceiptItemBatchId { get; set; }
    public long GoodsReceiptItemId { get; set; }
    public long BatchId { get; set; }
    public decimal Quantity { get; set; }

    public GoodsReceiptItem GoodsReceiptItem { get; set; } = null!;
    public Batch Batch { get; set; } = null!;
}
