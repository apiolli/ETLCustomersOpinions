using CustomersOpinions.Analytical.Application.Reporting;

namespace CustomersOpinions.Analytical.Application.Abstractions.Extraction;

public interface IExtractionPipeline
{
    Task<ExtractionReport> RunAsync(CancellationToken cancellationToken);
}
