namespace ThingRecommender.Application.Auth;

public record SignInRequest(string IdToken);

public record UserResponse(Guid Id, string DisplayName, string Email);

public record SignInResponse(string Token, UserResponse User);

/// <summary>The verified identity claimed by an external provider's ID token.</summary>
public record ExternalIdentity(string Provider, string ExternalId, string Email, string DisplayName);
