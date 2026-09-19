namespace StoreManagement.Domain.Entities;

public class StockAdjustment
{
    public Guid AdjustmentId { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public decimal SystemQuantity { get; set; }
    public decimal PhysicalQuantity { get; set; }
    public decimal Difference { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? ApprovedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ApprovedAt { get; set; }
    public bool IsApproved { get; set; }

    public Product Product { get; set; } = null!;
}
