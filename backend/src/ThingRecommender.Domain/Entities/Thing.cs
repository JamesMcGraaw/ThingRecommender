using ThingRecommender.Domain.Enums;

namespace ThingRecommender.Domain.Entities;

/// <summary>The film, book, restaurant, etc. that a recommendation points at.</summary>
public class Thing : Entity
{
    public required string Title { get; set; }
    public required MediaType MediaType { get; set; }

    /// <summary>Link to an external source (TMDB, IGDB, etc. depending on MediaType) - whichever
    /// provider looked it up, not tied to any one of them.</summary>
    public string? ExternalUrl { get; set; }
}
