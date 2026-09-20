namespace StoreManagement.Domain.Entities;

public class SaleItem
{
    public long SaleItemId { get; set; }
    public long SaleId { get; set; }
    public long ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public Sale Sale { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
