using System.ComponentModel.DataAnnotations;
using ThingRecommender.Domain.Enums;

namespace ThingRecommender.Application.Recommendations;

public record CreateRecommendationRequest(
    string RecipientEmail,
    string ThingTitle,
    MediaType MediaType,
    string? Note);

/// <summary>Logging a recommendation from someone who isn't a ThingRecommender user -
/// the current (authenticated) user is always the recipient.</summary>
public record LogManualRecommendationRequest(
    [Required, MinLength(1)] string ExternalRecommenderName,
    string ThingTitle,
    MediaType MediaType,
    string? Note);

public record RateRecommendationRequest([Range(1, 10)] int Score);

public record RecommendationResponse(
    Guid Id,
    Guid? RecommenderId,
    string RecommenderName,
    Guid RecipientId,
    string RecipientName,
    Guid ThingId,
    string ThingTitle,
    MediaType MediaType,
    string? ExternalUrl,
    string? Note,
    DateTime CreatedAtUtc,
    int? Score,
    DateTime? RatedAtUtc);

public record MediaTypeStrength(MediaType MediaType, double? AverageScore, int RatedCount);

/// <summary>
/// How well Recommender's "you'll love it" recommendations land with Recipient,
/// derived from the ratings Recipient has given so far — not stored, computed on demand.
/// ByMediaType only lists types with at least one rating - someone can have great taste in
/// films and mediocre taste in books, so the overall figure alone would hide that.
/// </summary>
public record RecommendationStrengthResponse(
    Guid RecommenderId,
    Guid RecipientId,
    double? AverageScore,
    int RatedCount,
    IReadOnlyList<MediaTypeStrength> ByMediaType);
