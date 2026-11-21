using CustomersOpinions.Analytical.Application.Abstractions.Extraction;
using CustomersOpinions.Analytical.Application.Extraction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CustomersOpinions.Analytical.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IExtractionPipeline, ExtractionPipeline>();
        return services;
    }
}
