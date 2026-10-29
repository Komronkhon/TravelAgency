using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using TravelAgency.Application.Interfaces;
using TravelAgency.Domain.Entities;

namespace TravelAgency.Infrastructure.Repositories;

public sealed class CachedTourRepository(
    TourRepository innerRepository,
    IDistributedCache cache,
    ILogger<CachedTourRepository> logger) : ITourRepository
{
    private const string AllToursCacheKey = "tours:all";
    private static string GetTourCacheKey(Guid id) => $"tours:{id}";

    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
        SlidingExpiration = TimeSpan.FromMinutes(2)
    };

    public async Task<Tour?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Для изменений (ReserveSeats) EF Core должен отслеживать сущность (ChangeTracker),
        // поэтому чтение по ID берём напрямую из MS SQL Server с проверкой RowVersion
        return await innerRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<Tour>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var cachedJson = await cache.GetStringAsync(AllToursCacheKey, cancellationToken);
            if (!string.IsNullOrEmpty(cachedJson))
            {
                logger.LogInformation("CACHE HIT (Redis): Список туров успешно прочитан из кеша за ~1 мс.");
                var cachedItems = JsonSerializer.Deserialize<List<TourCacheModel>>(cachedJson);
                if (cachedItems is not null)
                {
                    return cachedItems.Select(m => m.ToDomain()).ToList();
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis недоступен. Переключаемся на чтение напрямую из MS SQL Server.");
        }

        logger.LogInformation("CACHE MISS: Читаем список туров из MS SQL Server и сохраняем в Redis.");
        var tours = (await innerRepository.GetAllAsync(cancellationToken)).ToList();

        try
        {
            var cacheModels = tours.Select(TourCacheModel.FromDomain).ToList();
            var serialized = JsonSerializer.Serialize(cacheModels);
            await cache.SetStringAsync(AllToursCacheKey, serialized, CacheOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Не удалось записать список туров в Redis.");
        }

        return tours;
    }

    public async Task AddAsync(Tour tour, CancellationToken cancellationToken = default)
    {
        await innerRepository.AddAsync(tour, cancellationToken);
        await InvalidateCacheAsync(tour.Id, cancellationToken);
    }

    public async Task UpdateAsync(Tour tour, CancellationToken cancellationToken = default)
    {
        await innerRepository.UpdateAsync(tour, cancellationToken);
        await InvalidateCacheAsync(tour.Id, cancellationToken);
    }

    private async Task InvalidateCacheAsync(Guid tourId, CancellationToken ct)
    {
        try
        {
            await cache.RemoveAsync(AllToursCacheKey, ct);
            await cache.RemoveAsync(GetTourCacheKey(tourId), ct);
            logger.LogInformation("CACHE INVALIDATED: Сброшен кеш Redis после изменения тура {TourId}.", tourId);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Не удалось сбросить кеш Redis для тура {TourId}.", tourId);
        }
    }

    // Внутренняя модель сериализации для обхода приватных сеттеров Rich Domain Model
    private sealed record TourCacheModel(
        Guid Id,
        string Title,
        string Destination,
        decimal Price,
        int AvailableSeats,
        DateTime StartDate,
        DateTime EndDate)
    {
        public static TourCacheModel FromDomain(Tour t) =>
            new(t.Id, t.Title, t.Destination, t.Price, t.AvailableSeats, t.StartDate, t.EndDate);

        public Tour ToDomain()
        {
            var tour = new Tour(Title, Destination, Price, AvailableSeats, StartDate, EndDate);
            typeof(Tour).GetProperty(nameof(Tour.Id))?.SetValue(tour, Id);
            return tour;
        }
    }
}