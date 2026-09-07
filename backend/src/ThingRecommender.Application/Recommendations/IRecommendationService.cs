namespace ThingRecommender.Application.Recommendations;

public interface IRecommendationService
{
    /// <summary>Throws NotFoundException if no account exists for the recipient's email yet.</summary>
    Task<RecommendationResponse> CreateAsync(Guid recommenderId, CreateRecommendationRequest request, CancellationToken cancellationToken = default);

    /// <summary>Recommendations where currentUserId is the recommender or the recipient.</summary>
    Task<IReadOnlyList<RecommendationResponse>> GetForUserAsync(Guid currentUserId, CancellationToken cancellationToken = default);

    /// <summary>Throws NotFoundException if unknown, ForbiddenException if currentUserId isn't the recipient.</summary>
    Task<RecommendationResponse> RateAsync(Guid currentUserId, Guid recommendationId, int score, CancellationToken cancellationToken = default);

    /// <summary>Throws ForbiddenException unless currentUserId is one of the two parties.</summary>
    Task<RecommendationStrengthResponse> GetStrengthAsync(Guid currentUserId, Guid recommenderId, Guid recipientId, CancellationToken cancellationToken = default);
}
