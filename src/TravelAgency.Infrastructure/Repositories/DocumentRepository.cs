using Microsoft.EntityFrameworkCore;
using TravelAgency.Application.Interfaces;
using TravelAgency.Domain.Entities;
using TravelAgency.Infrastructure.Persistence;

namespace TravelAgency.Infrastructure.Repositories;

public sealed class DocumentRepository(TravelAgencyDbContext context) : IDocumentRepository
{
    public async Task<StoredDocument?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.StoredDocuments.FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<IReadOnlyList<StoredDocument>> GetByBookingIdAsync(Guid bookingId, CancellationToken ct = default) =>
        await context.StoredDocuments
            .AsNoTracking()
            .Where(d => d.BookingId == bookingId)
            .OrderByDescending(d => d.UploadedAtUtc)
            .ToListAsync(ct);

    public async Task AddAsync(StoredDocument document, CancellationToken ct = default) =>
        await context.StoredDocuments.AddAsync(document, ct);

    public async Task SaveChangesAsync(CancellationToken ct = default) =>
        await context.SaveChangesAsync(ct);
}