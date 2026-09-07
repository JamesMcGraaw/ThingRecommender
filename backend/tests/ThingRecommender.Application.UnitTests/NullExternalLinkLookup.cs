using ThingRecommender.Application.ExternalLinks;
using ThingRecommender.Domain.Enums;

namespace ThingRecommender.Application.UnitTests;

/// <summary>Test double that never finds a link, so tests don't depend on network access.</summary>
public class NullExternalLinkLookup : IExternalLinkLookup
{
    public Task<string?> TryFindUrlAsync(string title, MediaType mediaType, CancellationToken cancellationToken = default) =>
        Task.FromResult<string?>(null);
}

/// <summary>Test double that always finds the same URL, to verify callers use the lookup result.</summary>
public class StubExternalLinkLookup(string url) : IExternalLinkLookup
{
    public Task<string?> TryFindUrlAsync(string title, MediaType mediaType, CancellationToken cancellationToken = default) =>
        Task.FromResult<string?>(url);
}
