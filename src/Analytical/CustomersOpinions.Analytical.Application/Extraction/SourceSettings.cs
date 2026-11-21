namespace CustomersOpinions.Analytical.Application.Extraction;

/// <summary>Per source settings.</summary>
/// <param name="DisplayName">Name shown in logs and in the report (e.g. "Surveys (CSV)").</param>
/// <param name="Enabled">False to skip the source.</param>
public sealed record SourceSettings(string DisplayName, bool Enabled);
