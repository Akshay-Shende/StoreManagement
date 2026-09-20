namespace StoreManagement.Domain.Entities;

public class Customer
{
    public long CustomerId { get; set; }
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }

    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
