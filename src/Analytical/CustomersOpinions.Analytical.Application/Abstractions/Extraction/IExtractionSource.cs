using CustomersOpinions.Analytical.Application.Reporting;

namespace CustomersOpinions.Analytical.Application.Abstractions.Extraction;

public interface IExtractionSource
{
    string DisplayName { get; }

    string TableName { get; }

    bool IsEnabled { get; }

    Task<SourceExtractionResult> RunAsync(CancellationToken cancellationToken);
}
