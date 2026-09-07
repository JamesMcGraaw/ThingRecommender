using ThingRecommender.Domain.Enums;

namespace ThingRecommender.Application.Recommendations;

public record CreateRecommendationRequest(
    Guid RecommenderId,
    Guid RecipientId,
    string ThingTitle,
    MediaType MediaType,
    string? Note);

public record RecommendationResponse(
    Guid Id,
    Guid RecommenderId,
    Guid RecipientId,
    Guid ThingId,
    string ThingTitle,
    MediaType MediaType,
    string? Note,
    DateTime CreatedAtUtc,
    int? Score,
    DateTime? RatedAtUtc);
