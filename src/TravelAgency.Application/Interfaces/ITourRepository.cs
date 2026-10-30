using TravelAgency.Domain.Entities;

namespace TravelAgency.Application.Interfaces;

public interface ITourRepository
{
    Task<Tour?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tour>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Tour tour, CancellationToken cancellationToken = default);
    Task UpdateAsync(Tour tour, CancellationToken cancellationToken = default);
}