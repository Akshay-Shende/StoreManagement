namespace StoreManagement.Domain.Entities;

public class Notification
{
    public long NotificationId { get; set; }
    public string Type { get; set; } = "Info";
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = "Info";
    public long? ProductId { get; set; }
    public long? BatchId { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
