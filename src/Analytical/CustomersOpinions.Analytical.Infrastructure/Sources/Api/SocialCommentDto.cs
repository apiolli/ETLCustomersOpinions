namespace CustomersOpinions.Analytical.Infrastructure.Sources.Api;

public sealed record SocialCommentDto(
    string? CommentId,
    int? CustomerId,
    int? ProductId,
    string? Platform,
    string? CommentDate,
    string? Comment);
