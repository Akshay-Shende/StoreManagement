using StoreManagement.Application.DTOs;
namespace StoreManagement.Application.Interfaces;
public interface IAuditService
{
    Task WriteAsync(string action, string entityName, string? entityId, string actor, string? correlationId, string? ipAddress, object? before, object? after, string? reason, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AuditLogResponse>> GetAsync(int take, CancellationToken cancellationToken);
}
public record AuditLogResponse(long AuditLogId, string Action, string EntityName, string? EntityId, string Actor, string? CorrelationId, string? IpAddress, string? BeforeJson, string? AfterJson, string? Reason, DateTimeOffset CreatedAt);
