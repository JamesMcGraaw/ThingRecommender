using Microsoft.Extensions.DependencyInjection;
using ThingRecommender.Application.Recommendations;

namespace ThingRecommender.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IRecommendationService, RecommendationService>();

        return services;
    }
}
