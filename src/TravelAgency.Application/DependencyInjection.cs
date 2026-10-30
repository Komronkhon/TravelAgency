using Microsoft.Extensions.DependencyInjection;
using TravelAgency.Application.Auth;
using TravelAgency.Application.Tours;

namespace TravelAgency.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITourApplicationService, TourApplicationService>();
        services.AddScoped<IAuthApplicationService, AuthApplicationService>();
        return services;
    }
}