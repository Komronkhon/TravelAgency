using Microsoft.EntityFrameworkCore;
using TravelAgency.Application.Interfaces;
using TravelAgency.Domain.Entities;
using TravelAgency.Infrastructure.Persistence;

namespace TravelAgency.Infrastructure.Repositories;

public sealed class BookingRepository(TravelAgencyDbContext context) : IBookingRepository
{
    public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Bookings
            .Include(b => b.Tour)
            .FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public async Task<IReadOnlyList<Booking>> GetByCustomerEmailAsync(string email, CancellationToken ct = default)
    {
        return await context.Bookings
            .AsNoTracking()
            .Include(b => b.Tour)
            .Where(b => b.CustomerEmail == email)
            .OrderByDescending(b => b.CreatedAtUtc)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Booking booking, CancellationToken ct = default)
    {
        await context.Bookings.AddAsync(booking, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await context.SaveChangesAsync(ct);
    }
}