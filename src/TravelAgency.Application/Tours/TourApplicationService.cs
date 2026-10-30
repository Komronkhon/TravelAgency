using TravelAgency.Application.Interfaces;
using TravelAgency.Application.Tours.DTOs;
using TravelAgency.Domain.Common;
using TravelAgency.Domain.Entities;

namespace TravelAgency.Application.Tours;

public interface ITourApplicationService
{
    Task<IReadOnlyList<TourResponse>> GetAllAsync(CancellationToken ct = default);
    Task<Result<TourResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<Guid>> CreateAsync(CreateTourRequest request, CancellationToken ct = default);
    Task<Result<TourResponse>> ReserveSeatsAsync(Guid tourId, int seatsCount, CancellationToken ct = default);
}

public sealed class TourApplicationService(ITourRepository tourRepository) : ITourApplicationService
{
    public async Task<IReadOnlyList<TourResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var tours = await tourRepository.GetAllAsync(ct);
        return tours.Select(MapToResponse).ToList();
    }

    public async Task<Result<TourResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var tour = await tourRepository.GetByIdAsync(id, ct);
        if (tour is null)
        {
            return Result.Failure<TourResponse>(
                Error.NotFound("Tour.NotFound", $"Тур с ID '{id}' не найден."));
        }

        return Result.Success(MapToResponse(tour));
    }

    public async Task<Result<Guid>> CreateAsync(CreateTourRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Destination))
        {
            return Result.Failure<Guid>(
                Error.Validation("Tour.EmptyFields", "Название и направление тура обязательны для заполнения."));
        }

        if (request.Price <= 0)
        {
            return Result.Failure<Guid>(
                Error.Validation("Tour.InvalidPrice", "Цена тура должна быть строго больше нуля."));
        }

        if (request.StartDate >= request.EndDate)
        {
            return Result.Failure<Guid>(
                Error.Validation("Tour.InvalidDates", "Дата начала тура должна быть раньше даты окончания."));
        }

        var tour = new Tour(
            request.Title,
            request.Destination,
            request.Price,
            request.AvailableSeats,
            request.StartDate,
            request.EndDate);

        await tourRepository.AddAsync(tour, ct);
        return Result.Success(tour.Id);
    }

    public async Task<Result<TourResponse>> ReserveSeatsAsync(Guid tourId, int seatsCount, CancellationToken ct = default)
    {
        var tour = await tourRepository.GetByIdAsync(tourId, ct);
        if (tour is null)
        {
            return Result.Failure<TourResponse>(
                Error.NotFound("Tour.NotFound", $"Тур с ID '{tourId}' не найден."));
        }

        var reserveResult = tour.ReserveSeats(seatsCount);
        if (reserveResult.IsFailure)
        {
            return Result.Failure<TourResponse>(reserveResult.Error);
        }

        await tourRepository.UpdateAsync(tour, ct);
        return Result.Success(MapToResponse(tour));
    }

    private static TourResponse MapToResponse(Tour t) =>
        new(t.Id, t.Title, t.Destination, t.Price, t.AvailableSeats, t.StartDate, t.EndDate, t.IsActive);
}