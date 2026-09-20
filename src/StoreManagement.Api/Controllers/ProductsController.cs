using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Application.DTOs;
using StoreManagement.Application.Interfaces;

namespace StoreManagement.Api.Controllers;

[ApiController]
[Route("api/v1/products")]
[Authorize]
public sealed class ProductsController(IProductService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ProductResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await service.GetAllAsync(cancellationToken));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ProductResponse>> GetById(long id, CancellationToken cancellationToken) =>
        (await service.GetByIdAsync(id, cancellationToken)) is { } result ? Ok(result) : NotFound();

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.ProductId }, result);
    }

    [HttpPut("{id:long}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ProductResponse>> Update(long id, UpdateProductRequest request, CancellationToken cancellationToken) =>
        (await service.UpdateAsync(id, request, cancellationToken)) is { } result ? Ok(result) : NotFound();
}
