using StoreManagement.Domain.Enums;

namespace StoreManagement.Application.DTOs;

public sealed record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string FullName,
    UserRole Role = UserRole.Staff
);

public sealed record LoginRequest(
    string UsernameOrEmail,
    string Password
);

public sealed record RefreshTokenRequest(
    string AccessToken,
    string RefreshToken
);

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserInfoResponse User
);

public sealed record UserInfoResponse(
    Guid UserId,
    string Username,
    string Email,
    string FullName,
    string Role
);

public sealed record MessageResponse(
    string Message
);
