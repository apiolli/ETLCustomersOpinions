namespace CustomersOpinions.Transactional.Api.Contracts;

/// <summary>One social comment as published by the API.</summary>
/// <param name="CommentId">Source id (e.g. <c>T0001</c>).</param>
/// <param name="CustomerId">Customer id; null for anonymous comments.</param>
/// <param name="ProductId">Product id.</param>
/// <param name="Platform">Social network name.</param>
/// <param name="CommentDate">Date formatted as <c>yyyy-MM-dd</c>.</param>
/// <param name="Comment">Comment text.</param>
public sealed record SocialCommentResponse(
    string CommentId,
    int? CustomerId,
    int ProductId,
    string Platform,
    string CommentDate,
    string Comment);
