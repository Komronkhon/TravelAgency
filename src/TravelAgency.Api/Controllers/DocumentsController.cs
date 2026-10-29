using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelAgency.Application.Documents;

namespace TravelAgency.Api.Controllers;

[ApiVersion(1.0)]
[Authorize]
public sealed class DocumentsController(IDocubinetApplicationService docubinetService) : ApiControllerBase
{
    [HttpPost("bookings/{bookingId:guid}/generate-voucher")]
    [ProducesResponseType(typeof(DocumentMetadataResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerateVoucher(Guid bookingId, CancellationToken ct)
    {
        var result = await docubinetService.GenerateBookingVoucherAsync(bookingId, ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(DocumentMetadataResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Upload(
        IFormFile file,
        [FromQuery] Guid? bookingId,
        [FromQuery] string category = "PassportScan",
        CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("Файл не выбран или пуст.");
        }

        await using var stream = file.OpenReadStream();
        var result = await docubinetService.UploadDocumentAsync(
            bookingId,
            stream,
            file.FileName,
            file.ContentType,
            category,
            ct);

        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpGet("{documentId:guid}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(Guid documentId, CancellationToken ct)
    {
        var result = await docubinetService.DownloadDocumentAsync(documentId, ct);
        if (result.IsFailure)
        {
            return Problem(result.Error);
        }

        // FileStreamResult потоково передает байты в сокет Kestrel с поддержкой Range Processing!
        return File(result.Value.Stream, result.Value.ContentType, result.Value.FileName, enableRangeProcessing: true);
    }

    [HttpGet("bookings/{bookingId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<DocumentMetadataResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByBooking(Guid bookingId, CancellationToken ct)
    {
        var docs = await docubinetService.GetBookingDocumentsAsync(bookingId, ct);
        return Ok(docs);
    }
}