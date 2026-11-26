namespace CustomersOpinions.Analytical.Application.Reporting;

public sealed record DimensionLoadReport(DateTime StartedAt, TimeSpan Elapsed, IReadOnlyList<DimensionLoadResult> Results)
{
    public int TotalRead => Results.Sum(r => r.Read);
    public int TotalInserted => Results.Sum(r => r.Inserted);
    public int TotalUpdated => Results.Sum(r => r.Updated);
    public int TotalUnchanged => Results.Sum(r => r.Unchanged);
    public int TotalRejected => Results.Sum(r => r.Rejected);

    public bool AnyFailed => Results.Any(r => r.Status == SourceStatus.Failed);
}
