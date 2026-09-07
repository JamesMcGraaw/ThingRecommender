namespace ThingRecommender.Domain.Entities;

public class User : Entity
{
    public required string DisplayName { get; set; }
    public required string Email { get; set; }

    // Populated once real sign-in (Google/Microsoft) replaces the stubbed test users.
    public string? ExternalProvider { get; set; }
    public string? ExternalId { get; set; }
}
