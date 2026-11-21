namespace CustomersOpinions.Transactional.Data.Models;

public sealed class Survey
{
    public int OpinionId { get; set; }
    public int CustomerId { get; set; }
    public int ProductId { get; set; }
    public string? SourceId { get; set; }
    public DateTime Date { get; set; }
    public string Comment { get; set; } = string.Empty;
    public int ClassificationId { get; set; }
    public byte SatisfactionScore { get; set; }
}
