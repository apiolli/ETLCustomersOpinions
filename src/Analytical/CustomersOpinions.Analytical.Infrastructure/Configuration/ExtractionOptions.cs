using System.ComponentModel.DataAnnotations;

namespace CustomersOpinions.Analytical.Infrastructure.Configuration;

public sealed class ExtractionOptions
{
    public const string SectionName = "Extraction";

    [Range(1, 100_000)]
    public int BatchSize { get; set; } = 5000;

    [Range(1, 16)]
    public int MaxDegreeOfParallelism { get; set; } = 5;
}
