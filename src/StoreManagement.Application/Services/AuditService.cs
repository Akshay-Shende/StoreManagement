using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StoreManagement.Application.Interfaces;
using StoreManagement.Application.Interfaces.Data;
using StoreManagement.Domain.Entities;

namespace StoreManagement.Application.Services;

public sealed class AuditService(IStoreOperationsDataService data) : IAuditService
{
    public async Task WriteAsync(string action, string entityName, string? entityId, string actor, string? correlationId, string? ipAddress, object? before, object? after, string? reason, CancellationToken cancellationToken)
    {
        await data.AddAuditAsync(new AuditLog
        {
            Action = action, EntityName = entityName, EntityId = entityId, Actor = string.IsNullOrWhiteSpace(actor) ? "system" : actor,
            CorrelationId = correlationId, IpAddress = ipAddress, BeforeJson = before is null ? null : JsonSerializer.Serialize(before),
            AfterJson = after is null ? null : JsonSerializer.Serialize(after), Reason = reason
        }, cancellationToken);
        await data.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<AuditLogResponse>> GetAsync(int take, CancellationToken cancellationToken) =>
        await data.QueryAuditLogs().OrderByDescending(x => x.CreatedAt).Take(Math.Clamp(take, 1, 500))
            .Select(x => new AuditLogResponse(x.AuditLogId, x.Action, x.EntityName, x.EntityId, x.Actor, x.CorrelationId, x.IpAddress, x.BeforeJson, x.AfterJson, x.Reason, x.CreatedAt))
            .ToListAsync(cancellationToken);
}
