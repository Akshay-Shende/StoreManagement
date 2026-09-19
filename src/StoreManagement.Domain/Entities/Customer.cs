namespace StoreManagement.Domain.Entities;

public class Customer
{
    public Guid CustomerId { get; set; } = Guid.NewGuid();
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }

    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
