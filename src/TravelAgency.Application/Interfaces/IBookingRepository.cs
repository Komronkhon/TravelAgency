using TravelAgency.Domain.Entities;

namespace TravelAgency.Application.Interfaces;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Booking>> GetByCustomerEmailAsync(string email, CancellationToken ct = default);
    Task AddAsync(Booking booking, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}