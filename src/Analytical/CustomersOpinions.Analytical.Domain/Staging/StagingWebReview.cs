namespace CustomersOpinions.Analytical.Domain.Staging;

/// <summary>Web review read from the transactional database (table <c>Staging.WebReviews</c>).</summary>
public sealed record StagingWebReview : StagingRecord
{
    public string? ReviewId { get; init; }
    public string? CustomerId { get; init; }
    public string? ProductId { get; init; }
    public string? ReviewDate { get; init; }
    public string? Comment { get; init; }
    public string? Rating { get; init; }
}
