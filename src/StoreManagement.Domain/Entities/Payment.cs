namespace StoreManagement.Domain.Entities;

public class Payment
{
    public long PaymentId { get; set; }
    public long SaleId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = "Cash";
    public string Status { get; set; } = "Completed";
    public string? TransactionReference { get; set; }
    public string? ClientRequestId { get; set; }
    public DateTimeOffset PaidAt { get; set; } = DateTimeOffset.UtcNow;
    public string CreatedBy { get; set; } = "system";

    public Sale Sale { get; set; } = null!;
}
