using TravelAgency.Domain.Common;
using TravelAgency.Domain.Enums;

namespace TravelAgency.Domain.Entities;

public sealed class StoredDocument : BaseEntity
{
    public Guid? BookingId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long SizeInBytes { get; private set; }
    public string StorageKey { get; private set; } = string.Empty;
    public string Sha256Checksum { get; private set; } = string.Empty;
    public DocumentCategory Category { get; private set; }
    public DateTime UploadedAtUtc { get; private set; }

    public Booking? Booking { get; private set; }

    private StoredDocument() { }

    public StoredDocument(
        Guid? bookingId,
        string fileName,
        string contentType,
        long sizeInBytes,
        string storageKey,
        string sha256Checksum,
        DocumentCategory category)
    {
        BookingId = bookingId;
        FileName = fileName;
        ContentType = contentType;
        SizeInBytes = sizeInBytes;
        StorageKey = storageKey;
        Sha256Checksum = sha256Checksum;
        Category = category;
        UploadedAtUtc = DateTime.UtcNow;
    }
}