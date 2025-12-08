using System.Security.Claims;

namespace UMS_BE.Extensions;

public static class ControllerExtensions
{
    public static int? GetCurrentUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst("sub") ?? user.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return null;
    }
}
