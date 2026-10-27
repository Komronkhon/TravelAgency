using Microsoft.Extensions.DependencyInjection;
using TravelAgency.Application.Tours;

namespace TravelAgency.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITourApplicationService, TourApplicationService>();
        return services;
    }
}