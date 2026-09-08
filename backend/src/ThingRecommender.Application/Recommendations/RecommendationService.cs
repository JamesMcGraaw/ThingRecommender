using Microsoft.EntityFrameworkCore;
using ThingRecommender.Application.Abstractions;
using ThingRecommender.Application.Common;
using ThingRecommender.Application.ExternalLinks;
using ThingRecommender.Domain.Entities;
using ThingRecommender.Domain.Enums;

namespace ThingRecommender.Application.Recommendations;

public class RecommendationService(IApplicationDbContext db, IExternalLinkLookup externalLinkLookup) : IRecommendationService
{
    public async Task<RecommendationResponse> CreateAsync(Guid recommenderId, CreateRecommendationRequest request, CancellationToken cancellationToken = default)
    {
        var recommender = await db.Users.FirstOrDefaultAsync(u => u.Id == recommenderId, cancellationToken)
            ?? throw new NotFoundException($"No account with id {recommenderId}.");

        var recipient = await db.Users.FirstOrDefaultAsync(u => u.Email == request.RecipientEmail, cancellationToken)
            ?? throw new NotFoundException($"No account found for {request.RecipientEmail}.");

        var thing = await FindOrCreateThingAsync(request.ThingTitle, request.MediaType, cancellationToken);

        var recommendation = new Recommendation
        {
            RecommenderId = recommender.Id,
            RecipientId = recipient.Id,
            ThingId = thing.Id,
            Note = request.Note
        };
        db.Recommendations.Add(recommendation);

        await db.SaveChangesAsync(cancellationToken);

        return ToResponse(recommendation, thing, recommender, recipient);
    }

    public async Task<RecommendationResponse> LogManualAsync(Guid recipientId, LogManualRecommendationRequest request, CancellationToken cancellationToken = default)
    {
        var recipient = await db.Users.FirstOrDefaultAsync(u => u.Id == recipientId, cancellationToken)
            ?? throw new NotFoundException($"No account with id {recipientId}.");

        var thing = await FindOrCreateThingAsync(request.ThingTitle, request.MediaType, cancellationToken);

        var recommendation = new Recommendation
        {
            ExternalRecommenderName = request.ExternalRecommenderName,
            RecipientId = recipient.Id,
            ThingId = thing.Id,
            Note = request.Note
        };
        db.Recommendations.Add(recommendation);

        await db.SaveChangesAsync(cancellationToken);

        return ToResponse(recommendation, thing, recommender: null, recipient);
    }

    public async Task<IReadOnlyList<RecommendationResponse>> GetForUserAsync(Guid currentUserId, CancellationToken cancellationToken = default)
    {
        return await db.Recommendations
            .Include(r => r.Thing)
            .Include(r => r.Recommender)
            .Include(r => r.Recipient)
            .Where(r => r.RecommenderId == currentUserId || r.RecipientId == currentUserId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .Select(r => new RecommendationResponse(
                r.Id,
                r.RecommenderId,
                r.Recommender != null ? r.Recommender.DisplayName : r.ExternalRecommenderName!,
                r.RecipientId,
                r.Recipient!.DisplayName,
                r.ThingId,
                r.Thing!.Title,
                r.Thing.MediaType,
                r.Thing.ExternalUrl,
                r.Note,
                r.CreatedAtUtc,
                r.Score,
                r.RatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<RecommendationResponse> RateAsync(Guid currentUserId, Guid recommendationId, int score, CancellationToken cancellationToken = default)
    {
        var recommendation = await db.Recommendations
            .Include(r => r.Thing)
            .Include(r => r.Recommender)
            .Include(r => r.Recipient)
            .FirstOrDefaultAsync(r => r.Id == recommendationId, cancellationToken)
            ?? throw new NotFoundException($"No recommendation with id {recommendationId}.");

        if (recommendation.RecipientId != currentUserId)
        {
            throw new ForbiddenException("Only the recipient of a recommendation can rate it.");
        }

        recommendation.Score = score;
        recommendation.RatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return ToResponse(recommendation, recommendation.Thing!, recommendation.Recommender, recommendation.Recipient!);
    }

    public async Task<RecommendationStrengthResponse> GetStrengthAsync(Guid currentUserId, Guid recommenderId, Guid recipientId, CancellationToken cancellationToken = default)
    {
        if (currentUserId != recommenderId && currentUserId != recipientId)
        {
            throw new ForbiddenException("You can only view the strength score for a pair you're part of.");
        }

        var rated = await db.Recommendations
            .Where(r => r.RecommenderId == recommenderId && r.RecipientId == recipientId && r.Score != null)
            .Select(r => new { Score = r.Score!.Value, r.Thing!.MediaType })
            .ToListAsync(cancellationToken);

        var byMediaType = rated
            .GroupBy(r => r.MediaType)
            .Select(g => new MediaTypeStrength(g.Key, g.Average(r => (double)r.Score), g.Count()))
            .OrderBy(m => m.MediaType)
            .ToList();

        return new RecommendationStrengthResponse(
            recommenderId,
            recipientId,
            rated.Count > 0 ? rated.Average(r => (double)r.Score) : null,
            rated.Count,
            byMediaType);
    }

    private async Task<Thing> FindOrCreateThingAsync(string title, MediaType mediaType, CancellationToken cancellationToken)
    {
        var thing = await db.Things.FirstOrDefaultAsync(t => t.Title == title && t.MediaType == mediaType, cancellationToken);
        if (thing is not null)
        {
            return thing;
        }

        var externalUrl = await externalLinkLookup.TryFindUrlAsync(title, mediaType, cancellationToken);
        thing = new Thing { Title = title, MediaType = mediaType, ExternalUrl = externalUrl };
        db.Things.Add(thing);
        return thing;
    }

    private static RecommendationResponse ToResponse(Recommendation recommendation, Thing thing, User? recommender, User recipient) => new(
        recommendation.Id,
        recommendation.RecommenderId,
        recommender?.DisplayName ?? recommendation.ExternalRecommenderName!,
        recommendation.RecipientId,
        recipient.DisplayName,
        recommendation.ThingId,
        thing.Title,
        thing.MediaType,
        thing.ExternalUrl,
        recommendation.Note,
        recommendation.CreatedAtUtc,
        recommendation.Score,
        recommendation.RatedAtUtc);
}
