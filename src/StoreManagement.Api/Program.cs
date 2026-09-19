using Microsoft.EntityFrameworkCore;
using StoreManagement.Api.Middleware;
using StoreManagement.Application.Interfaces;
using StoreManagement.Application.Interfaces.Data;
using StoreManagement.Application.Services;
using StoreManagement.Infrastructure.Data;
using StoreManagement.Infrastructure.DataServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();

var connectionString = builder.Configuration.GetConnectionString("StoreDb")
    ?? throw new InvalidOperationException("ConnectionStrings:StoreDb is missing.");
// Use SQL Server provider instead of Npgsql
builder.Services.AddDbContext<StoreDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddScoped<IProductDataService, ProductDataService>();
builder.Services.AddScoped<ICategoryDataService, CategoryDataService>();
builder.Services.AddScoped<ISupplierDataService, SupplierDataService>();
builder.Services.AddScoped<IPurchaseDataService, PurchaseDataService>();
builder.Services.AddScoped<ISaleDataService, SaleDataService>();
builder.Services.AddScoped<IInventoryDataService, InventoryDataService>();
builder.Services.AddScoped<IDashboardDataService, DashboardDataService>();

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<ISaleService, SaleService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAiAssistantService, AiAssistantService>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
