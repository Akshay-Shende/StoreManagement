namespace StoreManagement.Domain.Entities;

public class SaleItem
{
    public Guid SaleItemId { get; set; } = Guid.NewGuid();
    public Guid SaleId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public Sale Sale { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
