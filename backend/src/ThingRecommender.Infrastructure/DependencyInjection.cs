using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using ThingRecommender.Application.Abstractions;
using ThingRecommender.Application.Auth;
using ThingRecommender.Application.ExternalLinks;
using ThingRecommender.Infrastructure.Auth;
using ThingRecommender.Infrastructure.ExternalLinks;
using ThingRecommender.Infrastructure.Persistence;

namespace ThingRecommender.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Connection string 'Database' is not configured.");

        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.Configure<TmdbOptions>(configuration.GetSection(TmdbOptions.SectionName));
        services.AddHttpClient<IExternalLinkLookup, TmdbExternalLinkLookup>(client =>
        {
            client.BaseAddress = new Uri("https://api.themoviedb.org/3/");
        });

        services.Configure<GoogleAuthOptions>(configuration.GetSection(GoogleAuthOptions.SectionName));
        services.Configure<MicrosoftAuthOptions>(configuration.GetSection(MicrosoftAuthOptions.SectionName));
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        var microsoftTenantId = configuration[$"{MicrosoftAuthOptions.SectionName}:TenantId"] ?? "common";
        services.AddSingleton(new ConfigurationManager<OpenIdConnectConfiguration>(
            $"https://login.microsoftonline.com/{microsoftTenantId}/v2.0/.well-known/openid-configuration",
            new OpenIdConnectConfigurationRetriever(),
            new HttpDocumentRetriever()));

        services.AddScoped<IExternalIdTokenValidator, GoogleIdTokenValidator>();
        services.AddScoped<IExternalIdTokenValidator, MicrosoftIdTokenValidator>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
