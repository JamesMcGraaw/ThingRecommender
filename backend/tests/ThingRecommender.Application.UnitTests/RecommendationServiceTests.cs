using ThingRecommender.Application.Recommendations;
using ThingRecommender.Domain.Entities;
using ThingRecommender.Domain.Enums;

namespace ThingRecommender.Application.UnitTests;

public class RecommendationServiceTests
{
    [Fact]
    public async Task CreateAsync_NewThing_SetsExternalUrlFromLookup()
    {
        using var db = TestDbContext.Create();
        var service = new RecommendationService(db, new StubExternalLinkLookup("https://www.themoviedb.org/movie/438631"));

        var result = await service.CreateAsync(new CreateRecommendationRequest(
            Guid.NewGuid(), Guid.NewGuid(), "Dune", MediaType.Film, null));

        Assert.Equal("https://www.themoviedb.org/movie/438631", result.ExternalUrl);
    }

    [Fact]
    public async Task CreateAsync_ExistingThing_DoesNotOverwriteExternalUrl()
    {
        using var db = TestDbContext.Create();
        var thing = new Thing { Title = "Dune", MediaType = MediaType.Film, ExternalUrl = "https://www.themoviedb.org/movie/438631" };
        db.Things.Add(thing);
        await db.SaveChangesAsync();

        // A lookup that would return something different, to prove it's never called for an existing Thing.
        var service = new RecommendationService(db, new StubExternalLinkLookup("https://example.com/wrong"));

        var result = await service.CreateAsync(new CreateRecommendationRequest(
            Guid.NewGuid(), Guid.NewGuid(), "Dune", MediaType.Film, null));

        Assert.Equal("https://www.themoviedb.org/movie/438631", result.ExternalUrl);
    }

    [Fact]
    public async Task RateAsync_SetsScoreAndRatedAtUtc()
    {
        using var db = TestDbContext.Create();
        var thing = new Thing { Title = "Dune", MediaType = MediaType.Film };
        var recommendation = new Recommendation
        {
            RecommenderId = Guid.NewGuid(),
            RecipientId = Guid.NewGuid(),
            ThingId = thing.Id
        };
        db.Things.Add(thing);
        db.Recommendations.Add(recommendation);
        await db.SaveChangesAsync();

        var service = new RecommendationService(db, new NullExternalLinkLookup());
        var result = await service.RateAsync(recommendation.Id, 8);

        Assert.NotNull(result);
        Assert.Equal(8, result!.Score);
        Assert.NotNull(result.RatedAtUtc);
    }

    [Fact]
    public async Task RateAsync_UnknownRecommendation_ReturnsNull()
    {
        using var db = TestDbContext.Create();
        var service = new RecommendationService(db, new NullExternalLinkLookup());

        var result = await service.RateAsync(Guid.NewGuid(), 5);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetStrengthAsync_AveragesOnlyRatedRecommendationsBetweenThePair()
    {
        using var db = TestDbContext.Create();
        var alice = Guid.NewGuid();
        var bob = Guid.NewGuid();
        var carol = Guid.NewGuid();
        var thing = new Thing { Title = "Dune", MediaType = MediaType.Film };
        db.Things.Add(thing);

        db.Recommendations.AddRange(
            new Recommendation { RecommenderId = alice, RecipientId = bob, ThingId = thing.Id, Score = 10 },
            new Recommendation { RecommenderId = alice, RecipientId = bob, ThingId = thing.Id, Score = 4 },
            new Recommendation { RecommenderId = alice, RecipientId = bob, ThingId = thing.Id, Score = null }, // not yet rated
            new Recommendation { RecommenderId = alice, RecipientId = carol, ThingId = thing.Id, Score = 1 }); // different recipient
        await db.SaveChangesAsync();

        var service = new RecommendationService(db, new NullExternalLinkLookup());
        var strength = await service.GetStrengthAsync(alice, bob);

        Assert.Equal(7, strength.AverageScore);
        Assert.Equal(2, strength.RatedCount);
    }

    [Fact]
    public async Task GetStrengthAsync_NoRatedRecommendations_ReturnsNullAverage()
    {
        using var db = TestDbContext.Create();
        var service = new RecommendationService(db, new NullExternalLinkLookup());

        var strength = await service.GetStrengthAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.Null(strength.AverageScore);
        Assert.Equal(0, strength.RatedCount);
    }
}
