using ThingRecommender.Domain.Entities;

namespace ThingRecommender.Domain.UnitTests;

public class RecommendationTests
{
    [Fact]
    public void NewRecommendation_HasNoScoreUntilRated()
    {
        var recommendation = new Recommendation
        {
            RecommenderId = Guid.NewGuid(),
            RecipientId = Guid.NewGuid(),
            ThingId = Guid.NewGuid()
        };

        Assert.Null(recommendation.Score);
        Assert.Null(recommendation.RatedAtUtc);
    }
}
