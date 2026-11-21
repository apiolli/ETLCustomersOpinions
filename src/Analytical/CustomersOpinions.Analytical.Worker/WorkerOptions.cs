using System.ComponentModel.DataAnnotations;

namespace CustomersOpinions.Analytical.Worker;

public enum RunMode
{
    Once,

    Interval
}

public sealed class WorkerOptions
{
    public const string SectionName = "Worker";

    public RunMode RunMode { get; set; } = RunMode.Once;

    [Range(1, 24 * 60)]
    public int IntervalMinutes { get; set; } = 60;
}
