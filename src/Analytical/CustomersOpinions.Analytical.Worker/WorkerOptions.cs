using System.ComponentModel.DataAnnotations;

namespace CustomersOpinions.Analytical.Worker;

/// <summary>How the worker runs the extraction.</summary>
public enum RunMode
{
    /// <summary>Run once, print the report and exit (console use).</summary>
    Once,

    /// <summary>Run every <see cref="WorkerOptions.IntervalMinutes"/> (Windows Service use).</summary>
    Interval
}

/// <summary>Section <c>Worker</c>.</summary>
public sealed class WorkerOptions
{
    public const string SectionName = "Worker";

    public RunMode RunMode { get; set; } = RunMode.Once;

    [Range(1, 24 * 60)]
    public int IntervalMinutes { get; set; } = 60;
}
