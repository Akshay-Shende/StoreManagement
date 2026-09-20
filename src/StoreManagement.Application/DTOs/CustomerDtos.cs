namespace StoreManagement.Application.DTOs;
public record CreateCustomerRequest(string Name, string? Phone = null, string? Email = null);
public record CustomerResponse(long CustomerId, string? Name, string? Phone, string? Email);
