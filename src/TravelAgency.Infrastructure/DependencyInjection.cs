using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelAgency.Application.Interfaces;
using TravelAgency.Infrastructure.Persistence;
using TravelAgency.Infrastructure.Repositories;

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

        return services;
    }
}