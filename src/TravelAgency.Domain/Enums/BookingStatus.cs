namespace TravelAgency.Domain.Enums;

public enum BookingStatus
{
    Pending = 1,    // Ожидает оплаты
    Confirmed = 2,  // Оплачено и подтверждено
    Cancelled = 3   // Отменено (места возвращены в тур)
}