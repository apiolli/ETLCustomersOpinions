namespace CustomersOpinions.Analytical.Domain.Staging;

/// <summary>Social network comment read from the REST API (table <c>Staging.SocialComments</c>).</summary>
public sealed record StagingSocialComment : StagingRecord
{
    public string? CommentId { get; init; }

    /// <summary>Null for anonymous comments.</summary>
    public string? CustomerId { get; init; }

    public string? ProductId { get; init; }
    public string? Platform { get; init; }
    public string? CommentDate { get; init; }
    public string? Comment { get; init; }
}
