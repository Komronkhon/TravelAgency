namespace TravelAgency.Application.Tours.DTOs;

public sealed record TourResponse(
    Guid Id,
    string Title,
    string Destination,
    decimal Price,
    int AvailableSeats,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive);

public sealed record CreateTourRequest(
    string Title,
    string Destination,
    decimal Price,
    int AvailableSeats,
    DateTime StartDate,
    DateTime EndDate);

public sealed record ReserveSeatsRequest(int SeatsCount);