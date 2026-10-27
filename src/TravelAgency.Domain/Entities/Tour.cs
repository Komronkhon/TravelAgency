using TravelAgency.Domain.Common;

namespace TravelAgency.Domain.Entities;

public sealed class Tour : BaseEntity
{
    public string Title { get; private set; }
    public string Destination { get; private set; }
    public decimal Price { get; private set; }
    public int AvailableSeats { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool IsActive { get; private set; }

    // Конструктор для EF Core
    private Tour() { }

    public Tour(string title, string destination, decimal price, int availableSeats, DateTime startDate, DateTime endDate)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Название тура не может быть пустым.", nameof(title));

        if (price <= 0)
            throw new ArgumentOutOfRangeException(nameof(price), "Цена тура должна быть больше нуля.");

        if (availableSeats < 0)
            throw new ArgumentOutOfRangeException(nameof(availableSeats), "Количество мест не может быть отрицательным.");

        if (startDate >= endDate)
            throw new ArgumentException("Дата начала тура должна быть раньше даты окончания.");

        Title = title;
        Destination = destination;
        Price = price;
        AvailableSeats = availableSeats;
        StartDate = startDate;
        EndDate = endDate;
        IsActive = true;
    }

    // Бизнес-метод бронирования мест в туре
    // Бизнес-метод бронирования мест в туре через Result Pattern
    public Result ReserveSeats(int count)
    {
        if (!IsActive)
        {
            return Result.Failure(Error.Conflict("Tour.Inactive", "Этот тур закрыт для бронирования."));
        }

        if (count <= 0)
        {
            return Result.Failure(Error.Validation("Tour.InvalidSeatCount", "Количество бронируемых мест должно быть больше нуля."));
        }

        if (AvailableSeats < count)
        {
            return Result.Failure(Error.Conflict(
                "Tour.NotEnoughSeats",
                $"Недостаточно свободных мест. Запрошено: {count}, доступно: {AvailableSeats}."));
        }

        AvailableSeats -= count;
        return Result.Success();
    }

    // Метод отмены брони / возврата мест
    public void IncreaseSeats(int count)
    {
        if (count <= 0)
            throw new ArgumentException("Количество возвращаемых мест должно быть больше нуля.", nameof(count));

        AvailableSeats += count;
    }
}