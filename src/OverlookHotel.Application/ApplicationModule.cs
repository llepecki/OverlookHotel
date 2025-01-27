namespace OverlookHotel.Application;

using Availability;
using Search;
using Microsoft.Extensions.DependencyInjection;

internal class ApplicationModuleLocator;

public static class ApplicationModule
{
    public static IServiceCollection AddApplicationComponents(this IServiceCollection services)
    {
        return services
            .AddMediatR(configuration => configuration
                .RegisterServicesFromAssemblyContaining<ApplicationModuleLocator>()
                .AddBehavior<AvailabilityQueryValidator>()
                .AddBehavior<SearchQueryValidator>());
    }
}