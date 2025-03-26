using MusicPlayerAPI.Exceptions;
using System.Security.Claims;

namespace MusicPlayerAPI.Helpers;

public static class UserHelper
{
    public static int GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst("Uid")?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
            //throw new UnauthorizedException("Invalid user ID in token.");
            return 0;

        return userId;
    }
}
