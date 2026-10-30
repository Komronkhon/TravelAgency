using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TravelAgency.Application.Bookings;

namespace TravelAgency.Api.Controllers;

[ApiVersion(1.0)]
public sealed class BookingsController(IBookingApplicationService bookingService) : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request, CancellationToken ct)
    {
        var result = await bookingService.CreateBookingAsync(request, ct);

        if (result.IsFailure)
            return Problem(result.Error);

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id, version = "1" }, result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await bookingService.GetByIdAsync(id, ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpGet("by-email")]
    public async Task<IActionResult> GetByEmail([FromQuery] string email, CancellationToken ct)
    {
        var bookings = await bookingService.GetByEmailAsync(email, ct);
        return Ok(bookings);
    }
}