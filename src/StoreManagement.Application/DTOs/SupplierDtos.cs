namespace StoreManagement.Application.DTOs;

public record CreateSupplierRequest(
    string Name,
    string? Phone,
    string? Email,
    string? Address,
    string? TaxRegistrationNumber);

public record SupplierResponse(
    Guid SupplierId,
    string Name,
    string? Phone,
    string? Email,
    string? Address,
    string? TaxRegistrationNumber);
