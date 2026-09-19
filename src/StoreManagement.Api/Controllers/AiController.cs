using Microsoft.AspNetCore.Mvc;
using StoreManagement.Application.DTOs;
using StoreManagement.Application.Interfaces;

namespace StoreManagement.Api.Controllers;

[ApiController]
[Route("api/v1/ai")]
public sealed class AiController(IAiAssistantService service) : ControllerBase
{
    [HttpPost("chat")]
    public async Task<ActionResult<AiChatResponse>> Chat(AiChatRequest request, CancellationToken cancellationToken) => Ok(await service.ChatAsync(request, cancellationToken));
}
