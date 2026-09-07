using ThingRecommender.Domain.Enums;

namespace ThingRecommender.Application.ExternalLinks;

/// <summary>
/// Looks up a link to an external source for a Thing (TMDB for films/TV, IGDB for games, etc).
/// Never throws - returns null if the media type isn't covered, no provider is configured, or
/// the lookup simply doesn't find anything, so a failed lookup never blocks creating a recommendation.
/// </summary>
public interface IExternalLinkLookup
{
    Task<string?> TryFindUrlAsync(string title, MediaType mediaType, CancellationToken cancellationToken = default);
}
