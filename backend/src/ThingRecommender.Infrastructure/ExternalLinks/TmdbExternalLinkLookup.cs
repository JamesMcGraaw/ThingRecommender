using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ThingRecommender.Application.ExternalLinks;
using ThingRecommender.Domain.Enums;

namespace ThingRecommender.Infrastructure.ExternalLinks;

/// <summary>Looks up films and TV shows on TMDB. Only covers those two media types for now -
/// other types get a null result until a matching provider (IGDB, Google Books, etc.) is added.</summary>
public class TmdbExternalLinkLookup(HttpClient httpClient, IOptions<TmdbOptions> options, ILogger<TmdbExternalLinkLookup> logger)
    : IExternalLinkLookup
{
    public async Task<string?> TryFindUrlAsync(string title, MediaType mediaType, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(options.Value.ApiKey))
        {
            return null;
        }

        var (searchPath, urlSegment) = mediaType switch
        {
            MediaType.Film => ("search/movie", "movie"),
            MediaType.TvShow => ("search/tv", "tv"),
            _ => (null, null)
        };

        if (searchPath is null)
        {
            return null;
        }

        try
        {
            var requestUri = $"{searchPath}?api_key={options.Value.ApiKey}&query={Uri.EscapeDataString(title)}";
            var response = await httpClient.GetAsync(requestUri, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("TMDB lookup for {Title} failed with status {StatusCode}", title, response.StatusCode);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<TmdbSearchResponse>(cancellationToken);
            var firstMatch = result?.Results.FirstOrDefault();

            return firstMatch is null ? null : $"https://www.themoviedb.org/{urlSegment}/{firstMatch.Id}";
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "TMDB lookup for {Title} failed", title);
            return null;
        }
    }

    private record TmdbSearchResponse(List<TmdbSearchResult> Results);

    private record TmdbSearchResult(int Id);
}
