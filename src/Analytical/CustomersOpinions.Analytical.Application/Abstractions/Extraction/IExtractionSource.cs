using CustomersOpinions.Analytical.Application.Reporting;

namespace CustomersOpinions.Analytical.Application.Abstractions.Extraction;

/// <summary>A configured source: an extractor paired with its staging table.</summary>
public interface IExtractionSource
{
    /// <summary>Display name used in logs and in the report.</summary>
    string DisplayName { get; }

    /// <summary>Staging table written by this source.</summary>
    string TableName { get; }

    /// <summary>False when the source is disabled by configuration.</summary>
    bool IsEnabled { get; }

    /// <summary>Extracts the source and refreshes its staging table.</summary>
    Task<SourceExtractionResult> RunAsync(CancellationToken cancellationToken);
}
