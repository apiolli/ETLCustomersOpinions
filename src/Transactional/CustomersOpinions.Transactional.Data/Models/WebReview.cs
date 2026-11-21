namespace CustomersOpinions.Transactional.Data.Models;

public sealed class WebReview
{
    public string ReviewId { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public int ProductId { get; set; }
    public string? SourceId { get; set; }
    public DateTime Date { get; set; }
    public string Comment { get; set; } = string.Empty;
    public byte Rating { get; set; }
    public int? ClassificationId { get; set; }
}
