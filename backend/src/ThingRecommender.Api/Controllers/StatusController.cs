using Microsoft.AspNetCore.Mvc;

namespace ThingRecommender.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatusController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "ok", timeUtc = DateTime.UtcNow });
}
