using System.ComponentModel.DataAnnotations;
using ThingRecommender.Domain.Enums;

namespace ThingRecommender.Application.Recommendations;

public record CreateRecommendationRequest(
    Guid RecommenderId,
    Guid RecipientId,
    string ThingTitle,
    MediaType MediaType,
    string? Note);

public record RateRecommendationRequest([Range(1, 10)] int Score);

public record RecommendationResponse(
    Guid Id,
    Guid RecommenderId,
    Guid RecipientId,
    Guid ThingId,
    string ThingTitle,
    MediaType MediaType,
    string? ExternalUrl,
    string? Note,
    DateTime CreatedAtUtc,
    int? Score,
    DateTime? RatedAtUtc);

/// <summary>
/// How well Recommender's "you'll love it" recommendations land with Recipient,
/// derived from the ratings Recipient has given so far — not stored, computed on demand.
/// </summary>
public record RecommendationStrengthResponse(
    Guid RecommenderId,
    Guid RecipientId,
    double? AverageScore,
    int RatedCount);
