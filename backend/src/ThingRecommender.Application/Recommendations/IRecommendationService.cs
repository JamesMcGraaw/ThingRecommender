namespace ThingRecommender.Application.Recommendations;

public interface IRecommendationService
{
    Task<RecommendationResponse> CreateAsync(CreateRecommendationRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RecommendationResponse>> GetAllAsync(CancellationToken cancellationToken = default);
}
