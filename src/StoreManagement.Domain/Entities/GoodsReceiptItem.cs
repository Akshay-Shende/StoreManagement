namespace StoreManagement.Domain.Entities;

public class GoodsReceiptItem
{
    public long GoodsReceiptItemId { get; set; }
    public long GoodsReceiptId { get; set; }
    public long PurchaseItemId { get; set; }
    public decimal Quantity { get; set; }

    public GoodsReceipt GoodsReceipt { get; set; } = null!;
    public PurchaseItem PurchaseItem { get; set; } = null!;
    public ICollection<GoodsReceiptItemBatch> Batches { get; set; } = new List<GoodsReceiptItemBatch>();
}
