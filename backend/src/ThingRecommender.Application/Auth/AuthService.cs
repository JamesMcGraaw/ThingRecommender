using Microsoft.EntityFrameworkCore;
using ThingRecommender.Application.Abstractions;
using ThingRecommender.Domain.Entities;

namespace ThingRecommender.Application.Auth;

public class AuthService(
    IApplicationDbContext db,
    IEnumerable<IExternalIdTokenValidator> validators,
    ITokenService tokenService) : IAuthService
{
    public async Task<SignInResponse?> SignInAsync(string provider, string idToken, CancellationToken cancellationToken = default)
    {
        var validator = validators.FirstOrDefault(v => v.Provider.Equals(provider, StringComparison.OrdinalIgnoreCase));
        if (validator is null)
        {
            return null;
        }

        var identity = await validator.ValidateAsync(idToken, cancellationToken);
        if (identity is null)
        {
            return null;
        }

        var user = await db.Users.FirstOrDefaultAsync(
            u => u.ExternalProvider == identity.Provider && u.ExternalId == identity.ExternalId,
            cancellationToken);

        if (user is null)
        {
            // Link to an existing account with the same email (e.g. one of the seeded test users)
            // rather than creating a duplicate.
            user = await db.Users.FirstOrDefaultAsync(u => u.Email == identity.Email, cancellationToken);
        }

        if (user is null)
        {
            user = new User
            {
                DisplayName = identity.DisplayName,
                Email = identity.Email,
                ExternalProvider = identity.Provider,
                ExternalId = identity.ExternalId
            };
            db.Users.Add(user);
        }
        else
        {
            user.ExternalProvider = identity.Provider;
            user.ExternalId = identity.ExternalId;
        }

        await db.SaveChangesAsync(cancellationToken);

        return new SignInResponse(tokenService.CreateToken(user), ToResponse(user));
    }

    public async Task<UserResponse?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        return user is null ? null : ToResponse(user);
    }

    private static UserResponse ToResponse(User user) => new(user.Id, user.DisplayName, user.Email);
}
