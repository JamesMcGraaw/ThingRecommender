using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThingRecommender.Api.Auth;
using ThingRecommender.Application.Auth;

namespace ThingRecommender.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("{provider}")]
    public async Task<ActionResult<SignInResponse>> SignIn(string provider, SignInRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.SignInAsync(provider, request.IdToken, cancellationToken);
        return result is null ? Unauthorized() : Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> Me(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await authService.GetUserAsync(userId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
