using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelAgency.Domain.Entities;

namespace TravelAgency.Infrastructure.Persistence.Configurations;

public sealed class TourConfiguration : IEntityTypeConfiguration<Tour>
{
    public void Configure(EntityTypeBuilder<Tour> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Destination)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(t => t.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(t => t.AvailableSeats)
            .IsRequired();

        builder.Property(t => t.StartDate)
            .IsRequired();

        builder.Property(t => t.EndDate)
            .IsRequired();

        builder.Property(t => t.IsActive)
            .IsRequired();

        // Включаем аппаратный rowversion в MS SQL Server для защиты от гонки потоков
        builder.Property(t => t.RowVersion)
            .IsRowVersion();
    }
}