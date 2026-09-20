namespace StoreManagement.Domain.Entities;

public class Return
{
    public long ReturnId { get; set; }
    public long SaleId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Condition { get; set; } = "Good";
    public decimal RefundAmount { get; set; }
    public string Status { get; set; } = "Completed";
    public string? ClientRequestId { get; set; }
    public string CreatedBy { get; set; } = "system";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Sale Sale { get; set; } = null!;
    public ICollection<ReturnItem> Items { get; set; } = new List<ReturnItem>();
}
