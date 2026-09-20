namespace StoreManagement.Domain.Entities;

public class GoodsReceipt
{
    public long GoodsReceiptId { get; set; }
    public long PurchaseId { get; set; }
    public string? ClientRequestId { get; set; }
    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;
    public string ReceivedBy { get; set; } = "system";
    public string? SupplierInvoiceNumber { get; set; }
    public string? Notes { get; set; }

    public Purchase Purchase { get; set; } = null!;
    public ICollection<GoodsReceiptItem> Items { get; set; } = new List<GoodsReceiptItem>();
}
