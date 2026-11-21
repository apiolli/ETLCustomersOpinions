namespace CustomersOpinions.Analytical.Application.Reporting;

/// <summary>Results of a complete extraction run.</summary>
public sealed record ExtractionReport(DateTime StartedAt, TimeSpan Elapsed, IReadOnlyList<SourceExtractionResult> Results)
{
    public int TotalRead => Results.Sum(r => r.Read);
    public int TotalWritten => Results.Sum(r => r.Written);
    public int TotalRejected => Results.Sum(r => r.Rejected);

    /// <summary>True when at least one enabled source failed.</summary>
    public bool AnyFailed => Results.Any(r => r.Status == SourceStatus.Failed);
}
