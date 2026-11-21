using System.ComponentModel.DataAnnotations;

namespace CustomersOpinions.Analytical.Infrastructure.Configuration;

/// <summary>Section <c>Extraction</c>.</summary>
public sealed class ExtractionOptions
{
    public const string SectionName = "Extraction";

    /// <summary>Rows per <c>SqlBulkCopy</c> batch.</summary>
    [Range(1, 100_000)]
    public int BatchSize { get; set; } = 5000;

    /// <summary>Sources running at the same time.</summary>
    [Range(1, 16)]
    public int MaxDegreeOfParallelism { get; set; } = 5;
}
