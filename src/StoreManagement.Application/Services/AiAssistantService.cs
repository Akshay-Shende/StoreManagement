using Microsoft.EntityFrameworkCore;
using StoreManagement.Application.DTOs;
using StoreManagement.Application.Interfaces;
using StoreManagement.Application.Interfaces.Data;

namespace StoreManagement.Application.Services;

// POC-safe AI facade: it only calls approved read-only application tools.
// A real provider (OpenAI/Gemini/Bedrock) can later call this service's tool methods.
public sealed class AiAssistantService(
    IInventoryDataService inventoryData,
    IProductDataService productData) : IAiAssistantService
{
    public async Task<AiChatResponse> ChatAsync(AiChatRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message)) throw new ArgumentException("Message is required.");
        var message = request.Message.Trim().ToLowerInvariant();

        if (message.Contains("low stock") || message.Contains("reorder"))
        {
            var lowStock = await inventoryData.QueryProducts().AsNoTracking().Where(x => x.IsActive && x.CurrentStock <= x.ReorderLevel)
                .OrderBy(x => x.CurrentStock).Select(x => new { x.ProductId, x.Name, x.CurrentStock, x.ReorderLevel, x.ReorderQuantity }).ToListAsync(cancellationToken);
            return new AiChatResponse($"{lowStock.Count} products are at or below their reorder level.", lowStock);
        }

        if (message.Contains("expire") || message.Contains("expiry"))
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var end = today.AddDays(30);
            var expiring = await inventoryData.QueryBatches().AsNoTracking().Where(x => x.IsSellable && x.AvailableQuantity > 0 && x.ExpiryDate.HasValue && x.ExpiryDate.Value <= end)
                .OrderBy(x => x.ExpiryDate).Select(x => new { x.Product.Name, x.BatchNumber, x.AvailableQuantity, x.ExpiryDate }).ToListAsync(cancellationToken);
            return new AiChatResponse($"I found {expiring.Count} batches expiring within 30 days or already expired.", expiring);
        }

        var product = await productData.Query().AsNoTracking().FirstOrDefaultAsync(x => x.IsActive && (message.Contains(x.Name.ToLower()) || (x.SKU != null && message.Contains(x.SKU.ToLower())) || (x.Barcode != null && message.Contains(x.Barcode.ToLower()))), cancellationToken);
        if (product is not null)
        {
            return new AiChatResponse($"{product.Name} currently has {product.CurrentStock} {product.Unit} in stock.", new { product.ProductId, product.Name, product.CurrentStock, product.Unit, product.ReorderLevel });
        }

        return new AiChatResponse("I can currently answer approved read-only questions about stock, low-stock/reorder items, and upcoming or expired inventory. The real LLM tool-calling layer can be plugged in without giving the model direct database access.");
    }
}
