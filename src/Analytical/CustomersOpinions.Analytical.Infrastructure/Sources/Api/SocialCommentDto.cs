namespace CustomersOpinions.Analytical.Infrastructure.Sources.Api;

/// <summary>Social comment as returned by <c>GET /api/v1/social-comments</c>.</summary>
public sealed record SocialCommentDto(
    string? CommentId,
    int? CustomerId,
    int? ProductId,
    string? Platform,
    string? CommentDate,
    string? Comment);
