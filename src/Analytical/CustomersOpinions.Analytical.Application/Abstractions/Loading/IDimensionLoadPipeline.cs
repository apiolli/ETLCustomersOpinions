using CustomersOpinions.Analytical.Application.Reporting;

namespace CustomersOpinions.Analytical.Application.Abstractions.Loading;

public interface IDimensionLoadPipeline
{
    Task<DimensionLoadReport> RunAsync(CancellationToken cancellationToken);
}
