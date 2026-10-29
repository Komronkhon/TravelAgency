using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelAgency.Application.Bookings;
using TravelAgency.Application.Interfaces;
using TravelAgency.Infrastructure.Authentication;
using TravelAgency.Infrastructure.Documents;
using TravelAgency.Infrastructure.Persistence;
using TravelAgency.Infrastructure.Repositories;
using TravelAgency.Infrastructure.Storage;

namespace TravelAgency.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Подключаем DbContext с MS SQL Server
        services.AddDbContext<TravelAgencyDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(TravelAgencyDbContext).Assembly.FullName)));

        // Регистрируем репозитории
        services.AddScoped<ITourRepository, TourRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IJwtProvider, JwtProvider>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IBookingApplicationService, BookingApplicationService>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddSingleton<IFileStorageService, LocalObjectStorageService>();
        services.AddSingleton<IVoucherPdfGenerator, QuestPdfVoucherGenerator>();

        return services;
    }
}