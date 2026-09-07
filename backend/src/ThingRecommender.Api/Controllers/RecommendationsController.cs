using Microsoft.AspNetCore.Mvc;
using ThingRecommender.Application.Recommendations;

namespace ThingRecommender.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecommendationsController(IRecommendationService recommendationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RecommendationResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await recommendationService.GetAllAsync(cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<RecommendationResponse>> Create(
        CreateRecommendationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await recommendationService.CreateAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPost("{id:guid}/rate")]
    public async Task<ActionResult<RecommendationResponse>> Rate(
        Guid id,
        RateRecommendationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await recommendationService.RateAsync(id, request.Score, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("strength")]
    public async Task<ActionResult<RecommendationStrengthResponse>> GetStrength(
        [FromQuery] Guid recommenderId,
        [FromQuery] Guid recipientId,
        CancellationToken cancellationToken)
    {
        return Ok(await recommendationService.GetStrengthAsync(recommenderId, recipientId, cancellationToken));
    }
}
