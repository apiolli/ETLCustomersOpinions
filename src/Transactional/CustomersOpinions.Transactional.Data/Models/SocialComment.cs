namespace CustomersOpinions.Transactional.Data.Models;

public sealed class SocialComment
{
    public string CommentId { get; set; } = string.Empty;
    public int? CustomerId { get; set; }
    public int ProductId { get; set; }
    public int SocialNetworkId { get; set; }
    public string? SourceId { get; set; }
    public DateTime Date { get; set; }
    public string Comment { get; set; } = string.Empty;
    public int? ClassificationId { get; set; }
}
