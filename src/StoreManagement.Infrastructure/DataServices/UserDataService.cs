using Microsoft.EntityFrameworkCore;
using StoreManagement.Application.Interfaces.Data;
using StoreManagement.Domain.Entities;
using StoreManagement.Infrastructure.Data;

namespace StoreManagement.Infrastructure.DataServices;

public sealed class UserDataService(StoreDbContext db) : IUserDataService
{
    public IQueryable<User> Query() => db.Users;

    public Task<User?> GetByIdAsync(long id, CancellationToken cancellationToken) =>
        db.Users.FirstOrDefaultAsync(x => x.UserId == id, cancellationToken);

    public Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken cancellationToken)
    {
        var normalized = usernameOrEmail.Trim().ToLowerInvariant();
        return db.Users.FirstOrDefaultAsync(
            x => x.Username.ToLower() == normalized || x.Email.ToLower() == normalized,
            cancellationToken);
    }

    public Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken) => db.Users.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken, cancellationToken);

    public Task<bool> ExistsByUsernameOrEmailAsync(string username, string email, CancellationToken cancellationToken)
    {
        var normalizedUsername = username.Trim().ToLowerInvariant();
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return db.Users.AnyAsync(
            x => x.Username.ToLower() == normalizedUsername || x.Email.ToLower() == normalizedEmail,
            cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken) =>
        await db.Users.AddAsync(user, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        db.SaveChangesAsync(cancellationToken);
}
