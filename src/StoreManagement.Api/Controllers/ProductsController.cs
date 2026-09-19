using Microsoft.AspNetCore.Mvc;
using StoreManagement.Application.DTOs;
using StoreManagement.Application.Interfaces;

namespace StoreManagement.Api.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(IProductService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ProductResponse>>> GetAll(CancellationToken cancellationToken) => Ok(await service.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductResponse>> GetById(Guid id, CancellationToken cancellationToken) =>
        (await service.GetByIdAsync(id, cancellationToken)) is { } result ? Ok(result) : NotFound();

    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.ProductId }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductResponse>> Update(Guid id, UpdateProductRequest request, CancellationToken cancellationToken) =>
        (await service.UpdateAsync(id, request, cancellationToken)) is { } result ? Ok(result) : NotFound();
}
