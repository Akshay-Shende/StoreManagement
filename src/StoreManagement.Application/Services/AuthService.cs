using System.Security.Claims;
using StoreManagement.Application.DTOs;
using StoreManagement.Application.Interfaces;
using StoreManagement.Application.Interfaces.Data;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Enums;

namespace StoreManagement.Application.Services;

public sealed class AuthService(
    IUserDataService userDataService,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            throw new ArgumentException("Username is required.");
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email is required.");
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            throw new ArgumentException("Password must be at least 6 characters long.");
        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new ArgumentException("Full name is required.");

        var exists = await userDataService.ExistsByUsernameOrEmailAsync(request.Username.Trim(), request.Email.Trim(), cancellationToken);
        if (exists)
            throw new InvalidOperationException("A user with this username or email already exists.");

        var user = new User
        {
            Username = request.Username.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            FullName = request.FullName.Trim(),
            PasswordHash = passwordHasher.HashPassword(request.Password),
            Role = Enum.IsDefined(typeof(UserRole), request.Role) ? request.Role : UserRole.Staff,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var (accessToken, expiresAt) = jwtTokenGenerator.GenerateAccessToken(user);
        var refreshToken = jwtTokenGenerator.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await userDataService.AddAsync(user, cancellationToken);
        await userDataService.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            accessToken,
            refreshToken,
            expiresAt,
            new UserInfoResponse(user.UserId, user.Username, user.Email, user.FullName, user.Role.ToString())
        );
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UsernameOrEmail) || string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Username/Email and password are required.");

        var user = await userDataService.GetByUsernameOrEmailAsync(request.UsernameOrEmail.Trim(), cancellationToken);
        if (user is null || !passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            throw new ArgumentException("Invalid username or password.");

        if (!user.IsActive)
            throw new InvalidOperationException("User account is inactive. Please contact your administrator.");

        var (accessToken, expiresAt) = jwtTokenGenerator.GenerateAccessToken(user);
        var refreshToken = jwtTokenGenerator.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await userDataService.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            accessToken,
            refreshToken,
            expiresAt,
            new UserInfoResponse(user.UserId, user.Username, user.Email, user.FullName, user.Role.ToString())
        );
    }

    public async Task LogoutAsync(long userId, CancellationToken cancellationToken)
    {
        var user = await userDataService.GetByIdAsync(userId, cancellationToken);
        if (user is not null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await userDataService.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.AccessToken) || string.IsNullOrWhiteSpace(request.RefreshToken))
            throw new ArgumentException("Access token and refresh token are required.");

        var principal = jwtTokenGenerator.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal is null)
            throw new ArgumentException("Invalid access token.");

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(userIdClaim, out var userId))
            throw new ArgumentException("Invalid user identifier in token.");

        var user = await userDataService.GetByIdAsync(userId, cancellationToken);
        if (user is null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            throw new ArgumentException("Invalid or expired refresh token.");

        if (!user.IsActive)
            throw new InvalidOperationException("User account is inactive.");

        var (newAccessToken, expiresAt) = jwtTokenGenerator.GenerateAccessToken(user);
        var newRefreshToken = jwtTokenGenerator.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await userDataService.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            newAccessToken,
            newRefreshToken,
            expiresAt,
            new UserInfoResponse(user.UserId, user.Username, user.Email, user.FullName, user.Role.ToString())
        );
    }

    public async Task<UserInfoResponse?> GetCurrentUserAsync(long userId, CancellationToken cancellationToken)
    {
        var user = await userDataService.GetByIdAsync(userId, cancellationToken);
        return user is null
            ? null
            : new UserInfoResponse(user.UserId, user.Username, user.Email, user.FullName, user.Role.ToString());
    }
}
