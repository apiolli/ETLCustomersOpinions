namespace CustomersOpinions.Analytical.Application.Reporting;

public sealed record DimensionLoadResult(
    string DimensionName,
    string TableName,
    SourceStatus Status,
    int Read,
    int Inserted,
    int Updated,
    int Unchanged,
    int Rejected,
    TimeSpan Elapsed,
    string Message)
{
    public static DimensionLoadResult Failed(string dimensionName, string tableName, TimeSpan elapsed, string message) =>
        new(dimensionName, tableName, SourceStatus.Failed, 0, 0, 0, 0, 0, elapsed, message);
}
