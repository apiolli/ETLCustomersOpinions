namespace CustomersOpinions.Analytical.Application.Reporting;

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
    public double RowsPerSecond => Elapsed.TotalSeconds > 0 ? Written / Elapsed.TotalSeconds : 0;

    public static SourceExtractionResult Skipped(string sourceName, string tableName) =>
        new(sourceName, tableName, SourceStatus.Skipped, 0, 0, 0, TimeSpan.Zero, "Disabled by configuration");

    public static SourceExtractionResult Failed(string sourceName, string tableName, TimeSpan elapsed, string message) =>
        new(sourceName, tableName, SourceStatus.Failed, 0, 0, 0, elapsed, message);
}
