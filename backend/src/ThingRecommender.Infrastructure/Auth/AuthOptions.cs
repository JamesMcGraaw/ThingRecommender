namespace ThingRecommender.Infrastructure.Auth;

public class GoogleAuthOptions
{
    public const string SectionName = "Authentication:Google";

    public string ClientId { get; set; } = string.Empty;
}

public class MicrosoftAuthOptions
{
    public const string SectionName = "Authentication:Microsoft";

    public string ClientId { get; set; } = string.Empty;

    /// <summary>"common" allows both personal and work/school Microsoft accounts.</summary>
    public string TenantId { get; set; } = "common";
}

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SigningKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "ThingRecommender";
    public string Audience { get; set; } = "ThingRecommender";
    public int ExpiryMinutes { get; set; } = 60 * 24 * 7;
}
