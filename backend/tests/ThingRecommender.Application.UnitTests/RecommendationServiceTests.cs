using ThingRecommender.Application.Common;
using ThingRecommender.Application.Recommendations;
using ThingRecommender.Domain.Entities;
using ThingRecommender.Domain.Enums;

namespace ThingRecommender.Application.UnitTests;

public class RecommendationServiceTests
{
    private static async Task<(TestDbContext Db, User Recommender, User Recipient)> SeedUsersAsync()
    {
        var db = TestDbContext.Create();
        var recommender = new User { DisplayName = "Alice", Email = "alice@example.com" };
        var recipient = new User { DisplayName = "Bob", Email = "bob@example.com" };
        db.Users.AddRange(recommender, recipient);
        await db.SaveChangesAsync();
        return (db, recommender, recipient);
    }

    [Fact]
    public async Task CreateAsync_NewThing_SetsExternalUrlFromLookup()
    {
        var (db, recommender, recipient) = await SeedUsersAsync();
        using var _ = db;
        var service = new RecommendationService(db, new StubExternalLinkLookup("https://www.themoviedb.org/movie/438631"));

        var result = await service.CreateAsync(recommender.Id, new CreateRecommendationRequest(
            recipient.Email, "Dune", MediaType.Film, null));

        Assert.Equal("https://www.themoviedb.org/movie/438631", result.ExternalUrl);
    }

    [Fact]
    public async Task CreateAsync_ExistingThing_DoesNotOverwriteExternalUrl()
    {
        var (db, recommender, recipient) = await SeedUsersAsync();
        using var _ = db;
        var thing = new Thing { Title = "Dune", MediaType = MediaType.Film, ExternalUrl = "https://www.themoviedb.org/movie/438631" };
        db.Things.Add(thing);
        await db.SaveChangesAsync();

        // A lookup that would return something different, to prove it's never called for an existing Thing.
        var service = new RecommendationService(db, new StubExternalLinkLookup("https://example.com/wrong"));

        var result = await service.CreateAsync(recommender.Id, new CreateRecommendationRequest(
            recipient.Email, "Dune", MediaType.Film, null));

        Assert.Equal("https://www.themoviedb.org/movie/438631", result.ExternalUrl);
    }

    [Fact]
    public async Task CreateAsync_UnknownRecipientEmail_ThrowsNotFound()
    {
        var (db, recommender, _) = await SeedUsersAsync();
        using var _ = db;
        var service = new RecommendationService(db, new NullExternalLinkLookup());

        await Assert.ThrowsAsync<NotFoundException>(() => service.CreateAsync(
            recommender.Id, new CreateRecommendationRequest("nobody@example.com", "Dune", MediaType.Film, null)));
    }

    [Fact]
    public async Task LogManualAsync_SetsExternalRecommenderNameAndNoRecommenderId()
    {
        var (db, _, recipient) = await SeedUsersAsync();
        using var _ = db;
        var service = new RecommendationService(db, new NullExternalLinkLookup());

        var result = await service.LogManualAsync(recipient.Id, new LogManualRecommendationRequest(
            "Nathan", "Dune", MediaType.Film, "he wouldn't stop talking about it"));

        Assert.Null(result.RecommenderId);
        Assert.Equal("Nathan", result.RecommenderName);
        Assert.Equal(recipient.Id, result.RecipientId);
    }

    [Fact]
    public async Task LogManualAsync_ThenRate_RecipientCanRateItLikeAnyOther()
    {
        var (db, _, recipient) = await SeedUsersAsync();
        using var _ = db;
        var service = new RecommendationService(db, new NullExternalLinkLookup());

        var logged = await service.LogManualAsync(recipient.Id, new LogManualRecommendationRequest(
            "Nathan", "Dune", MediaType.Film, null));
        var rated = await service.RateAsync(recipient.Id, logged.Id, 9);

        Assert.Equal(9, rated.Score);
        Assert.Equal("Nathan", rated.RecommenderName);
    }

    [Fact]
    public async Task RateAsync_SetsScoreAndRatedAtUtc()
    {
        var (db, recommender, recipient) = await SeedUsersAsync();
        using var _ = db;
        var thing = new Thing { Title = "Dune", MediaType = MediaType.Film };
        var recommendation = new Recommendation
        {
            RecommenderId = recommender.Id,
            RecipientId = recipient.Id,
            ThingId = thing.Id
        };
        db.Things.Add(thing);
        db.Recommendations.Add(recommendation);
        await db.SaveChangesAsync();

        var service = new RecommendationService(db, new NullExternalLinkLookup());
        var result = await service.RateAsync(recipient.Id, recommendation.Id, 8);

        Assert.Equal(8, result.Score);
        Assert.NotNull(result.RatedAtUtc);
    }

    [Fact]
    public async Task RateAsync_UnknownRecommendation_ThrowsNotFound()
    {
        var (db, _, recipient) = await SeedUsersAsync();
        using var _ = db;
        var service = new RecommendationService(db, new NullExternalLinkLookup());

        await Assert.ThrowsAsync<NotFoundException>(() => service.RateAsync(recipient.Id, Guid.NewGuid(), 5));
    }

    [Fact]
    public async Task RateAsync_NotTheRecipient_ThrowsForbidden()
    {
        var (db, recommender, recipient) = await SeedUsersAsync();
        using var _ = db;
        var thing = new Thing { Title = "Dune", MediaType = MediaType.Film };
        var recommendation = new Recommendation { RecommenderId = recommender.Id, RecipientId = recipient.Id, ThingId = thing.Id };
        db.Things.Add(thing);
        db.Recommendations.Add(recommendation);
        await db.SaveChangesAsync();

        var service = new RecommendationService(db, new NullExternalLinkLookup());

        // recommender tries to rate their own recommendation - only the recipient may.
        await Assert.ThrowsAsync<ForbiddenException>(() => service.RateAsync(recommender.Id, recommendation.Id, 8));
    }

    [Fact]
    public async Task GetForUserAsync_IncludesManualEntriesWithExternalName()
    {
        var (db, _, recipient) = await SeedUsersAsync();
        using var _ = db;
        var service = new RecommendationService(db, new NullExternalLinkLookup());
        await service.LogManualAsync(recipient.Id, new LogManualRecommendationRequest(
            "Nathan", "Dune", MediaType.Film, null));

        var results = await service.GetForUserAsync(recipient.Id);

        var entry = Assert.Single(results);
        Assert.Null(entry.RecommenderId);
        Assert.Equal("Nathan", entry.RecommenderName);
    }

    [Fact]
    public async Task GetStrengthAsync_AveragesOnlyRatedRecommendationsBetweenThePair()
    {
        var (db, alice, bob) = await SeedUsersAsync();
        using var _ = db;
        var carol = new User { DisplayName = "Carol", Email = "carol@example.com" };
        db.Users.Add(carol);
        var thing = new Thing { Title = "Dune", MediaType = MediaType.Film };
        db.Things.Add(thing);

        db.Recommendations.AddRange(
            new Recommendation { RecommenderId = alice.Id, RecipientId = bob.Id, ThingId = thing.Id, Score = 10 },
            new Recommendation { RecommenderId = alice.Id, RecipientId = bob.Id, ThingId = thing.Id, Score = 4 },
            new Recommendation { RecommenderId = alice.Id, RecipientId = bob.Id, ThingId = thing.Id, Score = null }, // not yet rated
            new Recommendation { RecommenderId = alice.Id, RecipientId = carol.Id, ThingId = thing.Id, Score = 1 }); // different recipient
        await db.SaveChangesAsync();

        var service = new RecommendationService(db, new NullExternalLinkLookup());
        var strength = await service.GetStrengthAsync(alice.Id, alice.Id, bob.Id);

        Assert.Equal(7, strength.AverageScore);
        Assert.Equal(2, strength.RatedCount);
    }

    [Fact]
    public async Task GetStrengthAsync_NoRatedRecommendations_ReturnsNullAverage()
    {
        var (db, alice, bob) = await SeedUsersAsync();
        using var _ = db;
        var service = new RecommendationService(db, new NullExternalLinkLookup());

        var strength = await service.GetStrengthAsync(alice.Id, alice.Id, bob.Id);

        Assert.Null(strength.AverageScore);
        Assert.Equal(0, strength.RatedCount);
    }

    [Fact]
    public async Task GetStrengthAsync_CurrentUserNotInPair_ThrowsForbidden()
    {
        var (db, alice, bob) = await SeedUsersAsync();
        using var _ = db;
        var outsider = new User { DisplayName = "Carol", Email = "carol@example.com" };
        db.Users.Add(outsider);
        await db.SaveChangesAsync();

        var service = new RecommendationService(db, new NullExternalLinkLookup());

        await Assert.ThrowsAsync<ForbiddenException>(() => service.GetStrengthAsync(outsider.Id, alice.Id, bob.Id));
    }
}
