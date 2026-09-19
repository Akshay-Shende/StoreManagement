using Microsoft.EntityFrameworkCore;
using StoreManagement.Application.DTOs;
using StoreManagement.Application.Interfaces;
using StoreManagement.Application.Interfaces.Data;
using StoreManagement.Domain.Entities;

namespace StoreManagement.Application.Services;

public sealed class SupplierService(ISupplierDataService data) : ISupplierService
{
    public async Task<IReadOnlyCollection<SupplierResponse>> GetAllAsync(CancellationToken cancellationToken) =>
        await data.Query().AsNoTracking().OrderBy(x => x.Name).Select(x => new SupplierResponse(x.SupplierId, x.Name, x.Phone, x.Email, x.Address, x.TaxRegistrationNumber)).ToListAsync(cancellationToken);

    public async Task<SupplierResponse> CreateAsync(CreateSupplierRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) throw new ArgumentException("Supplier name is required.");
        var supplier = new Supplier { Name = request.Name.Trim(), Phone = request.Phone?.Trim(), Email = request.Email?.Trim(), Address = request.Address?.Trim(), TaxRegistrationNumber = request.TaxRegistrationNumber?.Trim() };
        await data.AddAsync(supplier, cancellationToken);
        await data.SaveChangesAsync(cancellationToken);
        return new SupplierResponse(supplier.SupplierId, supplier.Name, supplier.Phone, supplier.Email, supplier.Address, supplier.TaxRegistrationNumber);
    }
}
