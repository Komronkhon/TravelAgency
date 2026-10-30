using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelAgency.Application.Tours;
using TravelAgency.Application.Tours.DTOs;

namespace TravelAgency.Api.Controllers;

[ApiVersion(1.0)]
public sealed class ToursController(ITourApplicationService tourService) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var tours = await tourService.GetAllAsync(ct);
        return Ok(tours);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await tourService.GetByIdAsync(id, ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Agent")]
    public async Task<IActionResult> Create([FromBody] CreateTourRequest request, CancellationToken ct)
    {
        var result = await tourService.CreateAsync(request, ct);

        if (result.IsFailure)
            return Problem(result.Error);

        return CreatedAtAction(nameof(GetById), new { id = result.Value, version = "1" }, new { id = result.Value });
    }

    [HttpPost("{id:guid}/reserve")]
    public async Task<IActionResult> ReserveSeats(Guid id, [FromBody] ReserveSeatsRequest request, CancellationToken ct)
    {
        var result = await tourService.ReserveSeatsAsync(id, request.SeatsCount, ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }
}