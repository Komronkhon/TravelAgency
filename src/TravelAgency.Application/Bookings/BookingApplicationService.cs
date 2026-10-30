using TravelAgency.Application.Common.Diagnostics;
using TravelAgency.Application.Interfaces;
using TravelAgency.Domain.Common;
using TravelAgency.Domain.Entities;

namespace TravelAgency.Application.Bookings;

public sealed record CreateBookingRequest(
    Guid TourId,
    string CustomerName,
    string CustomerEmail,
    int SeatsCount);

public sealed record BookingResponse(
    Guid Id,
    Guid TourId,
    string TourTitle,
    string CustomerName,
    string CustomerEmail,
    int SeatsCount,
    decimal TotalPrice,
    string Status,
    DateTime CreatedAtUtc);

public interface IBookingApplicationService
{
    Task<Result<BookingResponse>> CreateBookingAsync(CreateBookingRequest request, CancellationToken ct = default);
    Task<Result<BookingResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<BookingResponse>> GetByEmailAsync(string email, CancellationToken ct = default);
}

public sealed class BookingApplicationService(
    ITourRepository tourRepository,
    IBookingRepository bookingRepository,
    TravelAgencyMetrics metrics) : IBookingApplicationService
{
    public async Task<Result<BookingResponse>> CreateBookingAsync(CreateBookingRequest request, CancellationToken ct = default)
    {
        var tour = await tourRepository.GetByIdAsync(request.TourId, ct);

        if (tour is null)
            return Result.Failure<BookingResponse>(Error.NotFound("Tour.NotFound", $"Тур с ID '{request.TourId}' не найден."));

        var bookingResult = Booking.Create(tour, request.CustomerName, request.CustomerEmail, request.SeatsCount);

        if (bookingResult.IsFailure)
            return Result.Failure<BookingResponse>(bookingResult.Error);

        var booking = bookingResult.Value;

        try
        {
            await bookingRepository.AddAsync(booking, ct);
            await bookingRepository.SaveChangesAsync(ct);
            metrics.RecordBookingCreated(tour.Title, booking.SeatsCount, booking.TotalPrice);
        }
        catch (Exception ex) when (ex.GetType().Name == "DbUpdateConcurrencyException")
        {
            return Result.Failure<BookingResponse>(
                Error.Conflict(
                    "Booking.ConcurrencyConflict",
                    "Кто-то другой только что выкупил места в этом туре параллельно с вами. Пожалуйста, повторите попытку."));
        }

        return Result.Success(MapToResponse(booking, tour.Title));
    }

    public async Task<Result<BookingResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var booking = await bookingRepository.GetByIdAsync(id, ct);

        if (booking is null)
            return Result.Failure<BookingResponse>(Error.NotFound("Booking.NotFound", $"Бронь с ID '{id}' не найдена."));

        return Result.Success(MapToResponse(booking, booking.Tour?.Title ?? string.Empty));
    }

    public async Task<IReadOnlyList<BookingResponse>> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var bookings = await bookingRepository.GetByCustomerEmailAsync(email, ct);
        return bookings.Select(b => MapToResponse(b, b.Tour?.Title ?? string.Empty)).ToList();
    }

    private static BookingResponse MapToResponse(Booking b, string tourTitle) =>
        new(b.Id, b.TourId, tourTitle, b.CustomerName, b.CustomerEmail, b.SeatsCount, b.TotalPrice, b.Status.ToString(), b.CreatedAtUtc);
}