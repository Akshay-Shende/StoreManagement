using StoreManagement.Domain.Enums;

namespace StoreManagement.Domain.Entities;

public class Sale
{
    public long SaleId { get; set; }
    public DateTimeOffset SaleDate { get; set; } = DateTimeOffset.UtcNow;
    public long? CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public SaleStatus Status { get; set; } = SaleStatus.Draft;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Customer? Customer { get; set; }
    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
}
