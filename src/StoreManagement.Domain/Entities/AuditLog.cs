namespace StoreManagement.Domain.Entities;

public class AuditLog
{
    public long AuditLogId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string Actor { get; set; } = "system";
    public string? CorrelationId { get; set; }
    public string? IpAddress { get; set; }
    public string? BeforeJson { get; set; }
    public string? AfterJson { get; set; }
    public string? Reason { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
