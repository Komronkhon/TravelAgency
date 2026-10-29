using System.Security.Cryptography;
using TravelAgency.Application.Interfaces;
using TravelAgency.Domain.Common;
using TravelAgency.Domain.Entities;
using TravelAgency.Domain.Enums;

namespace TravelAgency.Application.Documents;

public sealed record DocumentMetadataResponse(
    Guid Id,
    Guid? BookingId,
    string FileName,
    string ContentType,
    long SizeInBytes,
    string Category,
    string Sha256Checksum,
    DateTime UploadedAtUtc);

public sealed record FileDownloadResult(Stream Stream, string FileName, string ContentType);

public interface IDocubinetApplicationService
{
    Task<Result<DocumentMetadataResponse>> GenerateBookingVoucherAsync(Guid bookingId, CancellationToken ct = default);
    Task<Result<DocumentMetadataResponse>> UploadDocumentAsync(
        Guid? bookingId,
        Stream fileStream,
        string fileName,
        string contentType,
        string category,
        CancellationToken ct = default);
    Task<Result<FileDownloadResult>> DownloadDocumentAsync(Guid documentId, CancellationToken ct = default);
    Task<IReadOnlyList<DocumentMetadataResponse>> GetBookingDocumentsAsync(Guid bookingId, CancellationToken ct = default);
}

public sealed class DocubinetApplicationService(
    IBookingRepository bookingRepository,
    IDocumentRepository documentRepository,
    IFileStorageService fileStorage,
    IVoucherPdfGenerator pdfGenerator) : IDocubinetApplicationService
{
    public async Task<Result<DocumentMetadataResponse>> GenerateBookingVoucherAsync(Guid bookingId, CancellationToken ct = default)
    {
        var booking = await bookingRepository.GetByIdAsync(bookingId, ct);
        if (booking is null || booking.Tour is null)
        {
            return Result.Failure<DocumentMetadataResponse>(
                Error.NotFound("Booking.NotFound", $"Бронь с ID '{bookingId}' не найдена."));
        }

        // 1. Генерируем официальный PDF-ваучер в памяти через QuestPDF
        var pdfBytes = pdfGenerator.GenerateVoucherPdf(booking, booking.Tour);
        var sha256 = Convert.ToHexStringLower(SHA256.HashData(pdfBytes));

        var fileName = $"TourVoucher_{booking.Id.ToString()[..8].ToUpperInvariant()}.pdf";
        var storageKey = $"vouchers/{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid()}_{fileName}";

        // 2. Потоково отправляем PDF в объектное хранилище
        using var memoryStream = new MemoryStream(pdfBytes);
        await fileStorage.UploadAsync(memoryStream, storageKey, "application/pdf", ct);

        // 3. Сохраняем карточку документа с SHA-256 чексуммой в MS SQL Server (Docubinet)
        var document = new StoredDocument(
            booking.Id,
            fileName,
            "application/pdf",
            pdfBytes.LongLength,
            storageKey,
            sha256,
            DocumentCategory.BookingVoucher);

        await documentRepository.AddAsync(document, ct);
        await documentRepository.SaveChangesAsync(ct);

        return Result.Success(MapToResponse(document));
    }

    public async Task<Result<DocumentMetadataResponse>> UploadDocumentAsync(
        Guid? bookingId,
        Stream fileStream,
        string fileName,
        string contentType,
        string category,
        CancellationToken ct = default)
    {
        if (!Enum.TryParse<DocumentCategory>(category, ignoreCase: true, out var docCategory))
        {
            docCategory = DocumentCategory.PassportScan;
        }

        // Вычисляем SHA-256 потоково и сбрасываем позицию потока в 0
        using var bufferStream = new MemoryStream();
        await fileStream.CopyToAsync(bufferStream, ct);
        bufferStream.Position = 0;

        var sha256 = Convert.ToHexStringLower(await SHA256.HashDataAsync(bufferStream, ct));
        bufferStream.Position = 0;

        var safeFileName = Path.GetFileName(fileName);
        var storageKey = $"uploads/{docCategory.ToString().ToLowerInvariant()}/{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid()}_{safeFileName}";

        await fileStorage.UploadAsync(bufferStream, storageKey, contentType, ct);

        var document = new StoredDocument(
            bookingId,
            safeFileName,
            contentType,
            bufferStream.Length,
            storageKey,
            sha256,
            docCategory);

        await documentRepository.AddAsync(document, ct);
        await documentRepository.SaveChangesAsync(ct);

        return Result.Success(MapToResponse(document));
    }

    public async Task<Result<FileDownloadResult>> DownloadDocumentAsync(Guid documentId, CancellationToken ct = default)
    {
        var doc = await documentRepository.GetByIdAsync(documentId, ct);
        if (doc is null)
        {
            return Result.Failure<FileDownloadResult>(
                Error.NotFound("Document.NotFound", $"Документ с ID '{documentId}' не найден в архиве Docubinet."));
        }

        var stream = await fileStorage.DownloadAsync(doc.StorageKey, ct);
        return Result.Success(new FileDownloadResult(stream, doc.FileName, doc.ContentType));
    }

    public async Task<IReadOnlyList<DocumentMetadataResponse>> GetBookingDocumentsAsync(Guid bookingId, CancellationToken ct = default)
    {
        var docs = await documentRepository.GetByBookingIdAsync(bookingId, ct);
        return docs.Select(MapToResponse).ToList();
    }

    private static DocumentMetadataResponse MapToResponse(StoredDocument d) =>
        new(d.Id, d.BookingId, d.FileName, d.ContentType, d.SizeInBytes, d.Category.ToString(), d.Sha256Checksum, d.UploadedAtUtc);
}