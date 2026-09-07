using Microsoft.EntityFrameworkCore;
using ThingRecommender.Application.Abstractions;
using ThingRecommender.Domain.Entities;

namespace ThingRecommender.Application.Recommendations;

public class RecommendationService(IApplicationDbContext db) : IRecommendationService
{
    public async Task<RecommendationResponse> CreateAsync(CreateRecommendationRequest request, CancellationToken cancellationToken = default)
    {
        var thing = await db.Things.FirstOrDefaultAsync(
            t => t.Title == request.ThingTitle && t.MediaType == request.MediaType,
            cancellationToken);

        if (thing is null)
        {
            thing = new Thing { Title = request.ThingTitle, MediaType = request.MediaType };
            db.Things.Add(thing);
        }

        var recommendation = new Recommendation
        {
            RecommenderId = request.RecommenderId,
            RecipientId = request.RecipientId,
            ThingId = thing.Id,
            Note = request.Note
        };
        db.Recommendations.Add(recommendation);

        await db.SaveChangesAsync(cancellationToken);

        return ToResponse(recommendation, thing);
    }

    public async Task<IReadOnlyList<RecommendationResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await db.Recommendations
            .Include(r => r.Thing)
            .OrderByDescending(r => r.CreatedAtUtc)
            .Select(r => new RecommendationResponse(
                r.Id,
                r.RecommenderId,
                r.RecipientId,
                r.ThingId,
                r.Thing!.Title,
                r.Thing.MediaType,
                r.Note,
                r.CreatedAtUtc,
                r.Score,
                r.RatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<RecommendationResponse?> RateAsync(Guid recommendationId, int score, CancellationToken cancellationToken = default)
    {
        var recommendation = await db.Recommendations
            .Include(r => r.Thing)
            .FirstOrDefaultAsync(r => r.Id == recommendationId, cancellationToken);

        if (recommendation is null)
        {
            return null;
        }

        recommendation.Score = score;
        recommendation.RatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return ToResponse(recommendation, recommendation.Thing!);
    }

    public async Task<RecommendationStrengthResponse> GetStrengthAsync(Guid recommenderId, Guid recipientId, CancellationToken cancellationToken = default)
    {
        var ratedScores = await db.Recommendations
            .Where(r => r.RecommenderId == recommenderId && r.RecipientId == recipientId && r.Score != null)
            .Select(r => r.Score!.Value)
            .ToListAsync(cancellationToken);

        return new RecommendationStrengthResponse(
            recommenderId,
            recipientId,
            ratedScores.Count > 0 ? ratedScores.Average() : null,
            ratedScores.Count);
    }

    private static RecommendationResponse ToResponse(Recommendation recommendation, Thing thing) => new(
        recommendation.Id,
        recommendation.RecommenderId,
        recommendation.RecipientId,
        recommendation.ThingId,
        thing.Title,
        thing.MediaType,
        recommendation.Note,
        recommendation.CreatedAtUtc,
        recommendation.Score,
        recommendation.RatedAtUtc);
}
