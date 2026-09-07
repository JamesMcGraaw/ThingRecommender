namespace ThingRecommender.Application.Recommendations;

public interface IRecommendationService
{
    Task<RecommendationResponse> CreateAsync(CreateRecommendationRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RecommendationResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns null if no recommendation with that ID exists.</summary>
    Task<RecommendationResponse?> RateAsync(Guid recommendationId, int score, CancellationToken cancellationToken = default);

    Task<RecommendationStrengthResponse> GetStrengthAsync(Guid recommenderId, Guid recipientId, CancellationToken cancellationToken = default);
}
