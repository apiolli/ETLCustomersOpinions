namespace CustomersOpinions.Analytical.Application.Reporting;

/// <summary>Final state of one source in a run.</summary>
public enum SourceStatus
{
    Succeeded,
    CompletedWithRejections,
    Failed,
    Skipped
}
