using ThingRecommender.Domain.Entities;

namespace ThingRecommender.Application.Auth;

/// <summary>Issues our own app-level JWT once an external sign-in has been verified.</summary>
public interface ITokenService
{
    string CreateToken(User user);
}
