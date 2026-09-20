using StoreManagement.Domain.Entities;

namespace StoreManagement.Application.Interfaces.Data;

public interface IUserDataService
{
    IQueryable<User> Query();
    Task<User?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken cancellationToken);
    Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
    Task<bool> ExistsByUsernameOrEmailAsync(string username, string email, CancellationToken cancellationToken);
    Task AddAsync(User user, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
