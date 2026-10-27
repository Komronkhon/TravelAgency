namespace TravelAgency.Domain.Enums;

public enum DocumentCategory
{
    BookingVoucher = 1, // Официальный PDF-ваучер на тур
    Contract = 2,       // Договор с туристом
    PassportScan = 3,   // Скан загранпаспорта
    FlightTicket = 4    // Маршрутная квитанция
}