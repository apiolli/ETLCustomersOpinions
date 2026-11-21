namespace CustomersOpinions.Analytical.Application.Reporting;

/// <summary>Counts and timing of one source.</summary>
public sealed record SourceExtractionResult(
    string SourceName,
    string TableName,
    SourceStatus Status,
    int Read,
    int Written,
    int Rejected,
    TimeSpan Elapsed,
    string Message)
{
    /// <summary>Rows written per second.</summary>
    public double RowsPerSecond => Elapsed.TotalSeconds > 0 ? Written / Elapsed.TotalSeconds : 0;

    /// <summary>Result of a source disabled by configuration.</summary>
    public static SourceExtractionResult Skipped(string sourceName, string tableName) =>
        new(sourceName, tableName, SourceStatus.Skipped, 0, 0, 0, TimeSpan.Zero, "Disabled by configuration");

    /// <summary>Result of a source that failed before producing counts.</summary>
    public static SourceExtractionResult Failed(string sourceName, string tableName, TimeSpan elapsed, string message) =>
        new(sourceName, tableName, SourceStatus.Failed, 0, 0, 0, elapsed, message);
}
