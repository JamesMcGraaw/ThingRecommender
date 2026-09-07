using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using ThingRecommender.Application.Auth;

namespace ThingRecommender.Infrastructure.Auth;

public class MicrosoftIdTokenValidator(
    ConfigurationManager<OpenIdConnectConfiguration> configurationManager,
    IOptions<MicrosoftAuthOptions> options,
    ILogger<MicrosoftIdTokenValidator> logger) : IExternalIdTokenValidator
{
    public string Provider => "Microsoft";

    public async Task<ExternalIdentity?> ValidateAsync(string idToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(options.Value.ClientId))
        {
            logger.LogWarning("Microsoft sign-in attempted but Authentication:Microsoft:ClientId is not configured.");
            return null;
        }

        try
        {
            var config = await configurationManager.GetConfigurationAsync(cancellationToken);

            var validationParameters = new TokenValidationParameters
            {
                // The "common" endpoint's issuer is tenant-specific (not a fixed string we can
                // compare against), so we rely on signature + audience + expiry instead - the
                // signing keys are still scoped to Microsoft's own trusted JWKS for this endpoint.
                ValidateIssuer = false,
                ValidateAudience = true,
                ValidAudience = options.Value.ClientId,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = config.SigningKeys
            };

            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(idToken, validationParameters, out _);

            var externalId = principal.FindFirst("oid")?.Value ?? principal.FindFirst("sub")?.Value;
            var email = principal.FindFirst("email")?.Value ?? principal.FindFirst("preferred_username")?.Value;
            var name = principal.FindFirst("name")?.Value ?? email;

            if (externalId is null || email is null)
            {
                logger.LogWarning("Microsoft ID token is missing expected claims (oid/sub, email)");
                return null;
            }

            return new ExternalIdentity(Provider, externalId, email, name ?? email);
        }
        catch (SecurityTokenException ex)
        {
            logger.LogWarning(ex, "Microsoft ID token failed validation");
            return null;
        }
    }
}
