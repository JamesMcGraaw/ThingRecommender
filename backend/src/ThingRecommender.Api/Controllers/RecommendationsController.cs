using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThingRecommender.Api.Auth;
using ThingRecommender.Application.Recommendations;

namespace ThingRecommender.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RecommendationsController(IRecommendationService recommendationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RecommendationResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await recommendationService.GetForUserAsync(User.GetUserId(), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<RecommendationResponse>> Create(
        CreateRecommendationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await recommendationService.CreateAsync(User.GetUserId(), request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPost("manual")]
    public async Task<ActionResult<RecommendationResponse>> LogManual(
        LogManualRecommendationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await recommendationService.LogManualAsync(User.GetUserId(), request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPost("{id:guid}/rate")]
    public async Task<ActionResult<RecommendationResponse>> Rate(
        Guid id,
        RateRecommendationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await recommendationService.RateAsync(User.GetUserId(), id, request.Score, cancellationToken);
        return Ok(result);
    }

    [HttpGet("strength")]
    public async Task<ActionResult<RecommendationStrengthResponse>> GetStrength(
        [FromQuery] Guid recommenderId,
        [FromQuery] Guid recipientId,
        CancellationToken cancellationToken)
    {
        return Ok(await recommendationService.GetStrengthAsync(User.GetUserId(), recommenderId, recipientId, cancellationToken));
    }
}
