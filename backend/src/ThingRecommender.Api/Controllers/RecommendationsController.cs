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
}
