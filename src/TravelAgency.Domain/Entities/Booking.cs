using TravelAgency.Domain.Common;
using TravelAgency.Domain.Enums;

namespace TravelAgency.Domain.Entities;

public sealed class Booking : BaseEntity
{
    public Guid TourId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string CustomerEmail { get; private set; } = string.Empty;
    public int SeatsCount { get; private set; }
    public decimal TotalPrice { get; private set; }
    public BookingStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    // Навигационное свойство для EF Core
    public Tour? Tour { get; private set; }

    private Booking() { }

    private Booking(
        Guid tourId,
        string customerName,
        string customerEmail,
        int seatsCount,
        decimal totalPrice)
    {
        TourId = tourId;
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        SeatsCount = seatsCount;
        TotalPrice = totalPrice;
        Status = BookingStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static Result<Booking> Create(
        Tour tour,
        string customerName,
        string customerEmail,
        int seatsCount)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            return Result.Failure<Booking>(Error.Validation("Booking.InvalidCustomer", "Имя туриста обязательно."));

        if (string.IsNullOrWhiteSpace(customerEmail) || !customerEmail.Contains('@'))
            return Result.Failure<Booking>(Error.Validation("Booking.InvalidEmail", "Указан некорректный Email туриста."));

        // Пытаемся зарезервировать места в самом туре
        var reserveResult = tour.ReserveSeats(seatsCount);
        if (reserveResult.IsFailure)
        {
            return Result.Failure<Booking>(reserveResult.Error);
        }

        var totalPrice = tour.Price * seatsCount;
        var booking = new Booking(tour.Id, customerName, customerEmail, seatsCount, totalPrice);

        return Result.Success(booking);
    }

    public Result Confirm()
    {
        if (Status == BookingStatus.Cancelled)
            return Result.Failure(Error.Conflict("Booking.AlreadyCancelled", "Нельзя подтвердить отменённую бронь."));

        Status = BookingStatus.Confirmed;
        return Result.Success();
    }

    public Result Cancel(Tour tour)
    {
        if (Status == BookingStatus.Cancelled)
            return Result.Failure(Error.Conflict("Booking.AlreadyCancelled", "Бронь уже была отменена ранее."));

        Status = BookingStatus.Cancelled;
        tour.IncreaseSeats(SeatsCount);
        return Result.Success();
    }
}