namespace ThingRecommender.Domain.Entities;

/// <summary>
/// One person recommending a Thing to another. Score is filled in once the
/// recipient watches/reads/visits it and rates it; the recommendation-strength
/// score between two people is derived from these ratings, not stored here.
///
/// The recommender doesn't have to be a ThingRecommender user - the recipient can
/// log a recommendation from someone off the platform (RecommenderId null,
/// ExternalRecommenderName set instead). Exactly one of the two is always set.
/// </summary>
public class Recommendation : Entity
{
    public Guid? RecommenderId { get; set; }
    public User? Recommender { get; set; }
    public string? ExternalRecommenderName { get; set; }

    public required Guid RecipientId { get; set; }
    public User? Recipient { get; set; }

    public required Guid ThingId { get; set; }
    public Thing? Thing { get; set; }

    public string? Note { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public int? Score { get; set; }
    public DateTime? RatedAtUtc { get; set; }
}
