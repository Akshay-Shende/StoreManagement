using Microsoft.AspNetCore.Mvc;
using StoreManagement.Application.DTOs;
using StoreManagement.Application.Interfaces;

namespace StoreManagement.Api.Controllers;

[ApiController]
[Route("api/categories")]
public sealed class CategoriesController(ICategoryService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CategoryResponse>>> GetAll(CancellationToken cancellationToken) => Ok(await service.GetAllAsync(cancellationToken));

    [HttpPost]
    public async Task<ActionResult<CategoryResponse>> Create(CreateCategoryRequest request, CancellationToken cancellationToken) => Ok(await service.CreateAsync(request, cancellationToken));
}
