using System.Security.Claims;

namespace CatCat.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static bool TryGetUserId(this ClaimsPrincipal user, out long userId)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (claim != null && long.TryParse(claim.Value, out userId))
        {
            return true;
        }
        userId = 0;
        return false;
    }

    public static long GetUserId(this ClaimsPrincipal user)
    {
        if (!user.TryGetUserId(out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in claims");
        }
        return userId;
    }

    public static string? GetRole(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Role)?.Value;
    }

    public static bool IsInRole(this ClaimsPrincipal user, string role)
    {
        return user.GetRole() == role;
    }
}

