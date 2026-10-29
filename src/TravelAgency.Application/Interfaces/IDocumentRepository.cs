using TravelAgency.Domain.Entities;

namespace TravelAgency.Application.Interfaces;

public interface IDocumentRepository
{
    Task<StoredDocument?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<StoredDocument>> GetByBookingIdAsync(Guid bookingId, CancellationToken ct = default);
    Task AddAsync(StoredDocument document, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}