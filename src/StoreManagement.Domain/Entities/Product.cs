namespace StoreManagement.Domain.Entities;

public class Product
{
    public Guid ProductId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? SKU { get; set; }
    public string? Barcode { get; set; }
    public Guid CategoryId { get; set; }
    public string Unit { get; set; } = "unit";
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal ReorderLevel { get; set; }
    public decimal ReorderQuantity { get; set; }
    public decimal CurrentStock { get; set; }
    public bool RequiresBatchTracking { get; set; }
    public bool IsActive { get; set; } = true;

    public Category Category { get; set; } = null!;
    public ICollection<Batch> Batches { get; set; } = new List<Batch>();
    public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    public ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
    public ICollection<StockAdjustment> StockAdjustments { get; set; } = new List<StockAdjustment>();
}
