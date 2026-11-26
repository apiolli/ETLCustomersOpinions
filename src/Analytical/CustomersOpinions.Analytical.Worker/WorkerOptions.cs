using System.ComponentModel.DataAnnotations;

namespace CustomersOpinions.Analytical.Worker;

public enum RunMode
{
    Once,

    Interval
}

public enum EtlStage
{
    All,

    Extraction,

    Dimensions
}

public sealed class WorkerOptions
{
    public const string SectionName = "Worker";

    public RunMode RunMode { get; set; } = RunMode.Once;

    public EtlStage Stage { get; set; } = EtlStage.All;

    [Range(1, 24 * 60)]
    public int IntervalMinutes { get; set; } = 60;
}
