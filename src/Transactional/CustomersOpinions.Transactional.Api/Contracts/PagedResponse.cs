namespace CustomersOpinions.Transactional.Api.Contracts;

/// <summary>A page of items plus the paging metadata.</summary>
/// <typeparam name="T">Item type.</typeparam>
public sealed record PagedResponse<T>(
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages,
    IReadOnlyList<T> Items)
{
    /// <summary>Builds the response computing <see cref="TotalPages"/>.</summary>
    public static PagedResponse<T> Create(int pageNumber, int pageSize, int totalCount, IReadOnlyList<T> items) =>
        new(pageNumber, pageSize, totalCount, (int)Math.Ceiling(totalCount / (double)pageSize), items);
}
