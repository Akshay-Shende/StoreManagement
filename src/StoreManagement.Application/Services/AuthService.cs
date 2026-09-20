using System.Security.Claims;
using StoreManagement.Application.DTOs;
using StoreManagement.Application.Interfaces;
using StoreManagement.Application.Interfaces.Data;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Enums;

namespace StoreManagement.Application.Services;

public sealed class AuthService(IUserDataService userDataService, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username)) throw new ArgumentException("Username is required.");
        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@')) throw new ArgumentException("A valid email is required.");
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8) throw new ArgumentException("Password must be at least 8 characters long.");
        if (string.IsNullOrWhiteSpace(request.FullName)) throw new ArgumentException("Full name is required.");
        if (await userDataService.ExistsByUsernameOrEmailAsync(request.Username.Trim(), request.Email.Trim(), cancellationToken)) throw new InvalidOperationException("A user with this username or email already exists.");

        var role = request.Role;
        var user = new User { Username = request.Username.Trim(), Email = request.Email.Trim().ToLowerInvariant(), FullName = request.FullName.Trim(), PasswordHash = passwordHasher.HashPassword(request.Password), Role = Enum.IsDefined(typeof(UserRole), role) ? role : UserRole.Staff, IsActive = true };
        await userDataService.AddAsync(user, cancellationToken);
        var response = IssueTokens(user);
        await userDataService.SaveChangesAsync(cancellationToken);
        return response;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UsernameOrEmail) || string.IsNullOrWhiteSpace(request.Password)) throw new ArgumentException("Username/Email and password are required.");
        var user = await userDataService.GetByUsernameOrEmailAsync(request.UsernameOrEmail.Trim(), cancellationToken);
        if (user is null || !passwordHasher.VerifyPassword(request.Password, user.PasswordHash)) throw new ArgumentException("Invalid username or password.");
        if (!user.IsActive) throw new InvalidOperationException("User account is inactive. Please contact an administrator.");
        var response = IssueTokens(user);
        await userDataService.SaveChangesAsync(cancellationToken);
        return response;
    }

    public async Task LogoutAsync(long userId, CancellationToken cancellationToken)
    {
        var user = await userDataService.GetByIdAsync(userId, cancellationToken);
        if (user is null) return;
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await userDataService.SaveChangesAsync(cancellationToken);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken)) throw new ArgumentException("Refresh session is required.");
        ClaimsPrincipal? principal = string.IsNullOrWhiteSpace(request.AccessToken) ? null : jwtTokenGenerator.GetPrincipalFromExpiredToken(request.AccessToken);
        long userId;
        if (principal is not null)
        {
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdClaim, out userId)) throw new ArgumentException("Invalid user identifier in token.");
        }
        else
        {
            var userByToken = await userDataService.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken) ?? throw new ArgumentException("Invalid refresh token.");
            userId = userByToken.UserId;
        }
        var user = await userDataService.GetByIdAsync(userId, cancellationToken);
        if (user is null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow) throw new ArgumentException("Invalid or expired refresh token.");
        if (!user.IsActive) throw new InvalidOperationException("User account is inactive.");
        var response = IssueTokens(user);
        await userDataService.SaveChangesAsync(cancellationToken);
        return response;
    }

    public async Task<UserInfoResponse?> GetCurrentUserAsync(long userId, CancellationToken cancellationToken)
    {
        var user = await userDataService.GetByIdAsync(userId, cancellationToken);
        return user is null ? null : new UserInfoResponse(user.UserId, user.Username, user.Email, user.FullName, user.Role.ToString(), user.IsActive);
    }

    private AuthResponse IssueTokens(User user)
    {
        var (accessToken, expiresAt) = jwtTokenGenerator.GenerateAccessToken(user);
        var refreshToken = jwtTokenGenerator.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        return new AuthResponse(accessToken, expiresAt, new UserInfoResponse(user.UserId, user.Username, user.Email, user.FullName, user.Role.ToString(), user.IsActive), refreshToken);
    }
}
