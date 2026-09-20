using System.Security.Claims;

namespace StoreManagement.Api.Authentication;

public static class ClaimsPrincipalExtensions
{
    public static string GetActorName(this ClaimsPrincipal user) =>
        user.Identity?.Name
        ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("The authenticated user has no identity claim.");
}
