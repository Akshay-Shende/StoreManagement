using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Application.DTOs;
using StoreManagement.Application.Interfaces;

namespace StoreManagement.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize(Roles = "Admin")]
public sealed class UsersController(IUserAdminService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<UserListResponse>>> GetAll(CancellationToken cancellationToken) => Ok(await service.GetAllAsync(cancellationToken));

    [HttpPut("{id:long}")]
    public async Task<ActionResult<UserInfoResponse>> Update(long id, UpdateUserRequest request, CancellationToken cancellationToken) => (await service.UpdateAsync(id, request, cancellationToken)) is { } result ? Ok(result) : NotFound();
}
