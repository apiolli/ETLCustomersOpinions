namespace CustomersOpinions.Analytical.Domain.Staging;

/// <summary>Internal survey read from the CSV file (table <c>Staging.Surveys</c>).</summary>
public sealed record StagingSurvey : StagingRecord
{
    public string? SurveyId { get; init; }
    public string? CustomerId { get; init; }
    public string? ProductId { get; init; }
    public string? SurveyDate { get; init; }
    public string? Comment { get; init; }
    public string? Classification { get; init; }
    public string? SatisfactionScore { get; init; }
    public string? Source { get; init; }
}
