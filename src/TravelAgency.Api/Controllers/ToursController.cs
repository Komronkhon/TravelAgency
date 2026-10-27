using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TravelAgency.Application.Tours;
using TravelAgency.Application.Tours.DTOs;

namespace TravelAgency.Api.Controllers;

[ApiVersion(1.0)]
public sealed class ToursController(ITourApplicationService tourService) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TourResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var tours = await tourService.GetAllAsync(ct);
        return Ok(tours);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TourResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await tourService.GetByIdAsync(id, ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTourRequest request, CancellationToken ct)
    {
        var result = await tourService.CreateAsync(request, ct);
        if (result.IsFailure)
        {
            return Problem(result.Error);
        }

        // По стандарту REST возвращаем 201 Created и заголовок Location со ссылкой на созданный ресурс
        return CreatedAtAction(nameof(GetById), new { id = result.Value, version = "1" }, new { id = result.Value });
    }

    [HttpPost("{id:guid}/reserve")]
    [ProducesResponseType(typeof(TourResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ReserveSeats(Guid id, [FromBody] ReserveSeatsRequest request, CancellationToken ct)
    {
        var result = await tourService.ReserveSeatsAsync(id, request.SeatsCount, ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }
}