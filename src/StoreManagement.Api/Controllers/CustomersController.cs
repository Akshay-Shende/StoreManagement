using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Application.DTOs;
using StoreManagement.Application.Interfaces;
namespace StoreManagement.Api.Controllers;
[ApiController]
[Route("api/v1/customers")]
[Authorize(Roles = "Admin,Manager,Cashier")]
public sealed class CustomersController(ICustomerService service) : ControllerBase
{
    [HttpGet] public async Task<ActionResult<IReadOnlyCollection<CustomerResponse>>> GetAll(CancellationToken cancellationToken) => Ok(await service.GetAllAsync(cancellationToken));
    [HttpPost] public async Task<ActionResult<CustomerResponse>> Create(CreateCustomerRequest request, CancellationToken cancellationToken) => Ok(await service.CreateAsync(request, cancellationToken));
}
