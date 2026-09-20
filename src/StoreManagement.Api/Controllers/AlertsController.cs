using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreManagement.Application.DTOs;
using StoreManagement.Application.Interfaces.Data;

namespace StoreManagement.Api.Controllers;

[ApiController]
[Route("api/v1/alerts")]
[Authorize]
public sealed class AlertsController(IStoreOperationsDataService data) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<NotificationResponse>>> Get(CancellationToken cancellationToken) =>
        Ok(await data.QueryNotifications().OrderByDescending(x => x.CreatedAt).Take(100).Select(x => new NotificationResponse(x.NotificationId, x.Type, x.Title, x.Message, x.Severity, x.ProductId, x.BatchId, x.IsRead, x.CreatedAt)).ToListAsync(cancellationToken));
}
