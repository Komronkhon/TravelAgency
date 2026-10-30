using TravelAgency.Domain.Common;

namespace TravelAgency.Domain.Entities;

public sealed class Tour : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Destination { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int AvailableSeats { get; private set; }
    public byte[] RowVersion { get; private set; } = [];
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool IsActive { get; private set; }

    private Tour() { }

    private Tour(
        string title,
        string destination,
        decimal price,
        int availableSeats,
        DateTime startDate,
        DateTime endDate)
    {
        Title = title;
        Destination = destination;
        Price = price;
        AvailableSeats = availableSeats;
        StartDate = startDate;
        EndDate = endDate;
        IsActive = true;
    }

    public static Result<Tour> Create(
        string title,
        string destination,
        decimal price,
        int availableSeats,
        DateTime startDate,
        DateTime endDate)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Failure<Tour>(Error.Validation(
                "Tour.InvalidTitle",
                "Название тура не может быть пустым."));
        }

        if (string.IsNullOrWhiteSpace(destination))
        {
            return Result.Failure<Tour>(Error.Validation(
                "Tour.InvalidDestination",
                "Место назначения не может быть пустым."));
        }

        if (price <= 0)
        {
            return Result.Failure<Tour>(Error.Validation(
                "Tour.InvalidPrice",
                "Цена тура должна быть больше нуля."));
        }

        if (availableSeats < 0)
        {
            return Result.Failure<Tour>(Error.Validation(
                "Tour.InvalidSeats",
                "Количество мест не может быть отрицательным."));
        }

        if (startDate >= endDate)
        {
            return Result.Failure<Tour>(Error.Validation(
                "Tour.InvalidDates",
                "Дата начала тура должна быть раньше даты окончания."));
        }
        
        var tour = new Tour(title, destination, price, availableSeats, startDate, endDate);

        return Result.Success(tour);
    }

    public Result ReserveSeats(int count)
    {
        if (!IsActive)
            return Result.Failure(Error.Conflict("Tour.Inactive", "Этот тур закрыт для бронирования."));

        if (count <= 0)
            return Result.Failure(Error.Validation("Tour.InvalidSeatCount", "Количество бронируемых мест должно быть больше нуля."));

        if (AvailableSeats < count)
            return Result.Failure(Error.Conflict(
                "Tour.NotEnoughSeats",
                $"Недостаточно свободных мест. Запрошено: {count}, доступно: {AvailableSeats}."));

        AvailableSeats -= count;
        return Result.Success();
    }

    public Result IncreaseSeats(int count)
    {
        if (count <= 0)
            return Result.Failure(Error.Conflict(
                "Tour.InvalidSeatCount",
                $"Количество возвращаемых мест должно быть больше нуля."));

        AvailableSeats += count;
        return Result.Success();
    }
}