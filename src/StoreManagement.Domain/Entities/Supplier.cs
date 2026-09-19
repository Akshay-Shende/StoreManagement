namespace StoreManagement.Domain.Entities;

public class Supplier
{
    public Guid SupplierId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxRegistrationNumber { get; set; }

    public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
}
