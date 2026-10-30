using System.Diagnostics.Metrics;

namespace TravelAgency.Application.Common.Diagnostics;

public sealed class TravelAgencyMetrics
{
    public const string MeterName = "TravelAgency.BusinessMetrics";

    private readonly Counter<long> _bookingsCreatedCounter;
    private readonly Counter<long> _seatsReservedCounter;
    private readonly Histogram<double> _bookingRevenueHistogram;

    public TravelAgencyMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);

        _bookingsCreatedCounter = meter.CreateCounter<long>(
            "travelagency.bookings.created",
            unit: "{booking}",
            description: "Общее количество успешно созданных броней");

        _seatsReservedCounter = meter.CreateCounter<long>(
            "travelagency.seats.reserved",
            unit: "{seat}",
            description: "Общее количество забронированных мест в турах");

        _bookingRevenueHistogram = meter.CreateHistogram<double>(
            "travelagency.bookings.revenue_usd",
            unit: "USD",
            description: "Распределение выручки по созданным броням в USD");
    }

    public void RecordBookingCreated(string tourTitle, int seatsCount, decimal totalPrice)
    {
        var tag = new KeyValuePair<string, object?>("tour.title", tourTitle);
        _bookingsCreatedCounter.Add(1, tag);
        _seatsReservedCounter.Add(seatsCount, tag);
        _bookingRevenueHistogram.Record((double)totalPrice, tag);
    }
}