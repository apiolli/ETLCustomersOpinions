namespace CustomersOpinions.Analytical.Infrastructure.Sources.Csv;

public sealed class SurveyFileRow
{
    public string? OpinionId { get; set; }
    public string? CustomerId { get; set; }
    public string? ProductId { get; set; }
    public string? Date { get; set; }
    public string? Comment { get; set; }
    public string? Classification { get; set; }
    public string? SatisfactionScore { get; set; }
    public string? Source { get; set; }
}
