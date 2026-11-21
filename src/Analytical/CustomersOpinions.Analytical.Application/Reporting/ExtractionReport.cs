namespace CustomersOpinions.Analytical.Application.Reporting;

public sealed record ExtractionReport(DateTime StartedAt, TimeSpan Elapsed, IReadOnlyList<SourceExtractionResult> Results)
{
    public int TotalRead => Results.Sum(r => r.Read);
    public int TotalWritten => Results.Sum(r => r.Written);
    public int TotalRejected => Results.Sum(r => r.Rejected);

    public bool AnyFailed => Results.Any(r => r.Status == SourceStatus.Failed);
}
