using Microsoft.EntityFrameworkCore;
using TravelAgency.Application.Interfaces;
using TravelAgency.Domain.Entities;
using TravelAgency.Infrastructure.Persistence;

namespace TravelAgency.Infrastructure.Repositories;

public sealed class TourRepository(TravelAgencyDbContext context) : ITourRepository
{
    public async Task<IEnumerable<Tour>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Tours.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<Tour?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Tours.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }    

    public async Task AddAsync(Tour tour, CancellationToken cancellationToken = default)
    {
        await context.Tours.AddAsync(tour, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Tour tour, CancellationToken cancellationToken = default)
    {
        context.Tours.Update(tour);
        await context.SaveChangesAsync(cancellationToken);
    }
}