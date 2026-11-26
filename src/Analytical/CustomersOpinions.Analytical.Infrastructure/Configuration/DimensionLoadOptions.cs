using System.ComponentModel.DataAnnotations;

namespace CustomersOpinions.Analytical.Infrastructure.Configuration;

public sealed class DimensionLoadOptions
{
    public const string SectionName = "DimensionLoad";

    [Range(1, 3600)]
    public int CommandTimeoutSeconds { get; set; } = 120;

    [Range(2, 20)]
    public int KeywordMinLength { get; set; } = 4;

    [Range(1, 10_000)]
    public int KeywordMinOccurrences { get; set; } = 2;

    public List<BusinessEventOptions> Events { get; set; } = new();
}

public sealed class BusinessEventOptions
{
    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }
}
