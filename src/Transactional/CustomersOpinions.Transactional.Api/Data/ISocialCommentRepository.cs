using CustomersOpinions.Transactional.Api.Contracts;

namespace CustomersOpinions.Transactional.Api.Data;

/// <summary>Read access to the social comments of <c>OpinionesClientesDB</c>.</summary>
public interface ISocialCommentRepository
{
    /// <summary>Returns one page of comments ordered by id, plus the total number of comments.</summary>
    Task<(IReadOnlyList<SocialCommentResponse> Items, int TotalCount)> GetPageAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken);

    /// <summary>Runs a short <c>SELECT 1</c> to check that the database is reachable.</summary>
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken);
}
