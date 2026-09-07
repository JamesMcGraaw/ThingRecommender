namespace ThingRecommender.Infrastructure.ExternalLinks;

public class TmdbOptions
{
    public const string SectionName = "Tmdb";

    /// <summary>TMDB v3 API key. Leave empty to disable TMDB lookups.</summary>
    public string ApiKey { get; set; } = string.Empty;
}
