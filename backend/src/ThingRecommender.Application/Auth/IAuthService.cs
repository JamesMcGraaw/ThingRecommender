namespace ThingRecommender.Application.Auth;

public interface IAuthService
{
    /// <summary>Returns null if the ID token doesn't validate against the named provider.</summary>
    Task<SignInResponse?> SignInAsync(string provider, string idToken, CancellationToken cancellationToken = default);

    Task<UserResponse?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
