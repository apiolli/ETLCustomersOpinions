namespace CustomersOpinions.Analytical.Domain.Staging;

/// <summary>
/// Base of every staging record: raw values as extracted plus the extraction metadata.
/// </summary>
public abstract record StagingRecord
{
    /// <summary>File, view or endpoint the record came from.</summary>
    public string SourceName { get; init; } = string.Empty;

    /// <summary>UTC time of the source run that extracted the record.</summary>
    public DateTime ExtractedAt { get; init; }

    /// <summary>Returns a copy of the record with the extraction metadata set.</summary>
    public StagingRecord WithMetadata(string sourceName, DateTime extractedAt) =>
        this with { SourceName = sourceName, ExtractedAt = extractedAt };
}
