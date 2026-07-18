using Mawasem.Application.Features.Authentication.Models;
using System.Globalization;
using System.Security.Claims;

namespace Mawasem.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static bool TryGetUserId(
        this ClaimsPrincipal principal ,
        out int userId )
    {
        ArgumentNullException.ThrowIfNull(principal);

        var userIdValue =
            principal.FindFirst(
                JwtClaimNames.Subject)?.Value;

        return int.TryParse(
            userIdValue ,
            NumberStyles.None ,
            CultureInfo.InvariantCulture ,
            out userId);
    }
}
