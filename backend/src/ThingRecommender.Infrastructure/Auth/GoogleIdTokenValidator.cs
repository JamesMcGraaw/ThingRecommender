using Google.Apis.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ThingRecommender.Application.Auth;

namespace ThingRecommender.Infrastructure.Auth;

public class GoogleIdTokenValidator(IOptions<GoogleAuthOptions> options, ILogger<GoogleIdTokenValidator> logger) : IExternalIdTokenValidator
{
    public string Provider => "Google";

    public async Task<ExternalIdentity?> ValidateAsync(string idToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(options.Value.ClientId))
        {
            logger.LogWarning("Google sign-in attempted but Authentication:Google:ClientId is not configured.");
            return null;
        }

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [options.Value.ClientId]
            });

            return new ExternalIdentity(Provider, payload.Subject, payload.Email, payload.Name);
        }
        catch (InvalidJwtException ex)
        {
            logger.LogWarning(ex, "Google ID token failed validation");
            return null;
        }
    }
}
