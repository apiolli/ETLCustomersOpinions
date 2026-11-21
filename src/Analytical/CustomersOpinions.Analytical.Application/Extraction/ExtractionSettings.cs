namespace CustomersOpinions.Analytical.Application.Extraction;

/// <summary>Pipeline wide settings (validated by Infrastructure when the options are bound).</summary>
/// <param name="BatchSize">Rows per bulk insert batch.</param>
/// <param name="MaxDegreeOfParallelism">Sources running at the same time.</param>
public sealed record ExtractionSettings(int BatchSize, int MaxDegreeOfParallelism);
