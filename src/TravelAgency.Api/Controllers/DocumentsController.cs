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
    public async Task<IActionResult> GenerateVoucher(Guid bookingId, CancellationToken ct)
    {
        var result = await docubinetService.GenerateBookingVoucherAsync(bookingId, ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(
        IFormFile file,
        [FromQuery] Guid? bookingId,
        [FromQuery] string category = "PassportScan",
        CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            return BadRequest("Файл не выбран или пуст.");

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
    public async Task<IActionResult> Download(Guid documentId, CancellationToken ct)
    {
        var result = await docubinetService.DownloadDocumentAsync(documentId, ct);

        if (result.IsFailure)
            return Problem(result.Error);

        return File(result.Value.Stream, result.Value.ContentType, result.Value.FileName, enableRangeProcessing: true);
    }

    [HttpGet("bookings/{bookingId:guid}")]
    public async Task<IActionResult> GetByBooking(Guid bookingId, CancellationToken ct)
    {
        var docs = await docubinetService.GetBookingDocumentsAsync(bookingId, ct);
        return Ok(docs);
    }
}