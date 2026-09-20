using StoreManagement.Domain.Enums;

namespace StoreManagement.Domain.Entities;

public class Purchase
{
    public long PurchaseId { get; set; }
    public long SupplierId { get; set; }
    public DateTimeOffset PurchaseDate { get; set; } = DateTimeOffset.UtcNow;
    public PurchaseStatus Status { get; set; } = PurchaseStatus.Draft;
    public decimal TotalAmount { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Supplier Supplier { get; set; } = null!;
    public ICollection<PurchaseItem> Items { get; set; } = new List<PurchaseItem>();
}
