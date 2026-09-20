using Microsoft.EntityFrameworkCore;
using StoreManagement.Application.DTOs;
using StoreManagement.Application.Interfaces;
using StoreManagement.Application.Interfaces.Data;

namespace StoreManagement.Application.Services;

public sealed class UserAdminService(IUserDataService data) : IUserAdminService
{
    public async Task<IReadOnlyCollection<UserListResponse>> GetAllAsync(CancellationToken cancellationToken) =>
        await data.Query().AsNoTracking().OrderBy(x => x.Username).Select(x => new UserListResponse(x.UserId, x.Username, x.Email, x.FullName, x.Role.ToString(), x.IsActive, x.CreatedAt)).ToListAsync(cancellationToken);

    public async Task<UserInfoResponse?> UpdateAsync(long id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await data.GetByIdAsync(id, cancellationToken);
        if (user is null) return null;
        if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Email)) throw new ArgumentException("Full name and email are required.");
        user.FullName = request.FullName.Trim();
        user.Email = request.Email.Trim().ToLowerInvariant();
        user.Role = request.Role;
        user.IsActive = request.IsActive;
        if (!user.IsActive) { user.RefreshToken = null; user.RefreshTokenExpiryTime = null; }
        await data.SaveChangesAsync(cancellationToken);
        return new UserInfoResponse(user.UserId, user.Username, user.Email, user.FullName, user.Role.ToString(), user.IsActive);
    }
}
