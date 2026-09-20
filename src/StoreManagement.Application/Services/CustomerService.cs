using Microsoft.EntityFrameworkCore;
using StoreManagement.Application.DTOs;
using StoreManagement.Application.Interfaces;
using StoreManagement.Application.Interfaces.Data;
using StoreManagement.Domain.Entities;

namespace StoreManagement.Application.Services;
public sealed class CustomerService(ICustomerDataService data) : ICustomerService
{
    public async Task<IReadOnlyCollection<CustomerResponse>> GetAllAsync(CancellationToken cancellationToken) => await data.Query().AsNoTracking().OrderBy(x => x.Name).Select(x => new CustomerResponse(x.CustomerId, x.Name, x.Phone, x.Email)).ToListAsync(cancellationToken);
    public async Task<CustomerResponse> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) throw new ArgumentException("Customer name is required.");
        var customer = new Customer { Name = request.Name.Trim(), Phone = request.Phone?.Trim(), Email = request.Email?.Trim().ToLowerInvariant() };
        await data.AddAsync(customer, cancellationToken); await data.SaveChangesAsync(cancellationToken);
        return new CustomerResponse(customer.CustomerId, customer.Name, customer.Phone, customer.Email);
    }
}
