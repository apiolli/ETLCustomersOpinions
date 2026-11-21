namespace CustomersOpinions.Analytical.Domain.Staging;

public sealed record StagingSocialComment : StagingRecord
{
    public string? CommentId { get; init; }

    public string? CustomerId { get; init; }

    public string? ProductId { get; init; }
    public string? Platform { get; init; }
    public string? CommentDate { get; init; }
    public string? Comment { get; init; }
}
