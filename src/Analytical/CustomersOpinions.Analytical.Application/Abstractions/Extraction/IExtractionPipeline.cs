using CustomersOpinions.Analytical.Application.Reporting;

namespace CustomersOpinions.Analytical.Application.Abstractions.Extraction;

/// <summary>Runs every registered source and builds the run report.</summary>
public interface IExtractionPipeline
{
    /// <summary>Runs the enabled sources in parallel; a failing source never stops the others.</summary>
    Task<ExtractionReport> RunAsync(CancellationToken cancellationToken);
}
