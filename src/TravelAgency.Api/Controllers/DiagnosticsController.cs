using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace TravelAgency.Api.Controllers;

[ApiVersion(1.0)]
public sealed class DiagnosticsController(IHostEnvironment environment) : ApiControllerBase
{
    [HttpGet("status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetPlatformStatus()
    {
        return Ok(new
        {
            Application = "TravelAgency Enterprise API",
            Architecture = "Clean Architecture + MVC Controllers",
            DatabaseProvider = "Microsoft SQL Server",
            Environment = environment.EnvironmentName,
            RuntimeVersion = Environment.Version.ToString(),
            TraceId = HttpContext.TraceIdentifier,
            TimestampUtc = DateTime.UtcNow
        });
    }
}