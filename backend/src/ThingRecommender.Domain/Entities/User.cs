namespace ThingRecommender.Domain.Entities;

public class User : Entity
{
    public required string DisplayName { get; set; }
    public required string Email { get; set; }

    // "Google" or "Microsoft" - which provider ExternalId came from.
    public string? ExternalProvider { get; set; }
    public string? ExternalId { get; set; }
}
