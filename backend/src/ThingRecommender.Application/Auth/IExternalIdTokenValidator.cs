namespace ThingRecommender.Application.Auth;

/// <summary>
/// Validates an ID token issued by one external identity provider (Google, Microsoft, ...).
/// Register one implementation per provider; ExternalAuthService picks the right one by
/// matching Provider against the request. Returns null (never throws) for an invalid/expired
/// token, so a bad token just looks like "sign-in failed" to the caller.
/// </summary>
public interface IExternalIdTokenValidator
{
    string Provider { get; }

    Task<ExternalIdentity?> ValidateAsync(string idToken, CancellationToken cancellationToken = default);
}
