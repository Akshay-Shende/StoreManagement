using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Application.DTOs;
using StoreManagement.Application.Interfaces;

namespace StoreManagement.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(IAuthService authService, IConfiguration configuration) : ControllerBase
{
    private const string RefreshCookie = "store_refresh_token";
    private CookieOptions CookieOptions => new()
    {
        HttpOnly = true,
        Secure = Request.IsHttps,
        SameSite = SameSiteMode.Lax,
        Path = "/api/v1/auth",
        Expires = DateTimeOffset.UtcNow.AddDays(configuration.GetValue("JwtSettings:RefreshTokenExpiryDays", 7))
    };

    [HttpPost("signup")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AuthResponse>> SignUp(RegisterRequest request, CancellationToken cancellationToken)
    {
        var response = await authService.RegisterAsync(request, cancellationToken);
        SetRefreshCookie(response.RefreshToken);
        return Ok(response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> LogIn(LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await authService.LoginAsync(request, cancellationToken);
        SetRefreshCookie(response.RefreshToken);
        return Ok(response);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<MessageResponse>> LogOut(CancellationToken cancellationToken)
    {
        if (long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) await authService.LogoutAsync(userId, cancellationToken);
        Response.Cookies.Delete(RefreshCookie, new CookieOptions { HttpOnly = true, Secure = Request.IsHttps, SameSite = SameSiteMode.Lax, Path = "/api/v1/auth" });
        return Ok(new MessageResponse("Logged out successfully."));
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue(RefreshCookie, out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken)) return Unauthorized(new { error = "Refresh session is missing." });
        var response = await authService.RefreshTokenAsync(request with { RefreshToken = refreshToken }, cancellationToken);
        SetRefreshCookie(response.RefreshToken);
        return Ok(response);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserInfoResponse>> GetCurrentUser(CancellationToken cancellationToken)
    {
        if (!long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Unauthorized(new { error = "Invalid token claims." });
        var user = await authService.GetCurrentUserAsync(userId, cancellationToken);
        return user is not null ? Ok(user) : NotFound();
    }

    private void SetRefreshCookie(string token) => Response.Cookies.Append(RefreshCookie, token, CookieOptions);
}
