using StoreManagement.Application.DTOs;

namespace StoreManagement.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task LogoutAsync(long userId, CancellationToken cancellationToken);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken);
    Task<UserInfoResponse?> GetCurrentUserAsync(long userId, CancellationToken cancellationToken);
}
