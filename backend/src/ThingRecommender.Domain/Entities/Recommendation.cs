namespace ThingRecommender.Domain.Entities;

/// <summary>
/// One person recommending a Thing to another. Score is filled in once the
/// recipient watches/reads/visits it and rates it; the recommendation-strength
/// score between two people is derived from these ratings, not stored here.
/// </summary>
public class Recommendation : Entity
{
    public required Guid RecommenderId { get; set; }
    public User? Recommender { get; set; }

    public required Guid RecipientId { get; set; }
    public User? Recipient { get; set; }

    public required Guid ThingId { get; set; }
    public Thing? Thing { get; set; }

    public string? Note { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public int? Score { get; set; }
    public DateTime? RatedAtUtc { get; set; }
}
