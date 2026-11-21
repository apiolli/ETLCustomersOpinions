namespace CustomersOpinions.Analytical.Domain.Staging;

public abstract record StagingRecord
{
    public string SourceName { get; init; } = string.Empty;

    public DateTime ExtractedAt { get; init; }

    public StagingRecord WithMetadata(string sourceName, DateTime extractedAt) =>
        this with { SourceName = sourceName, ExtractedAt = extractedAt };
}
