namespace CustomersOpinions.Transactional.Api.Contracts;

public sealed record SocialCommentResponse(
    string CommentId,
    int? CustomerId,
    int ProductId,
    string Platform,
    string CommentDate,
    string Comment);
