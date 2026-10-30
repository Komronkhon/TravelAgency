using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TravelAgency.Application.Interfaces;
using TravelAgency.Domain.Entities;

namespace TravelAgency.Infrastructure.Documents;

public sealed class QuestPdfVoucherGenerator : IVoucherPdfGenerator
{
    public QuestPdfVoucherGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateVoucherPdf(Booking booking, Tour tour)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(12));

                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("TRAVEL AGENCY ENTERPRISE").FontSize(22).Bold().FontColor(Colors.Blue.Darken2);
                        col.Item().Text("Официальный туристический ваучер (Docubinet Verified)").FontSize(10).FontColor(Colors.Grey.Darken1);
                    });

                    row.ConstantItem(140).AlignRight().Column(col =>
                    {
                        col.Item().Text($"ВАУЧЕР #{booking.Id.ToString()[..8].ToUpperInvariant()}").Bold();
                        col.Item().Text($"Статус: {booking.Status}").FontColor(Colors.Green.Darken2);
                    });
                });

                page.Content().PaddingVertical(25).Column(col =>
                {
                    col.Spacing(12);

                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    col.Item().Text("Информация о туре").FontSize(16).Bold();
                    col.Item().Text($"Название тура: {tour.Title}");
                    col.Item().Text($"Направление: {tour.Destination}");
                    col.Item().Text($"Даты поездки: {tour.StartDate:dd.MM.yyyy} — {tour.EndDate:dd.MM.yyyy}");

                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    col.Item().Text("Данные туриста и бронирования").FontSize(16).Bold();
                    col.Item().Text($"ФИО Заказчика: {booking.CustomerName}");
                    col.Item().Text($"Email: {booking.CustomerEmail}");
                    col.Item().Text($"Количество мест: {booking.SeatsCount}");
                    col.Item().Text($"Итоговая стоимость: ${booking.TotalPrice:N2}").FontSize(14).Bold().FontColor(Colors.Blue.Darken3);
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Сгенерировано автоматически в системе Docubinet UTC: ");
                    x.Span($"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
                });
            });
        });

        return document.GeneratePdf();
    }
}