using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ThingRecommender.Api.Auth;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? throw new InvalidOperationException("No sub claim on the authenticated user.");

        return Guid.Parse(value);
    }
}
