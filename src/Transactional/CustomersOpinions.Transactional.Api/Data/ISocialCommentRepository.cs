using CustomersOpinions.Transactional.Api.Contracts;

namespace CustomersOpinions.Transactional.Api.Data;

public interface ISocialCommentRepository
{
    Task<(IReadOnlyList<SocialCommentResponse> Items, int TotalCount)> GetPageAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken);

    Task<bool> IsHealthyAsync(CancellationToken cancellationToken);
}
