namespace CustomersOpinions.Transactional.Api.Contracts;

public sealed record PagedResponse<T>(
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages,
    IReadOnlyList<T> Items)
{
    public static PagedResponse<T> Create(int pageNumber, int pageSize, int totalCount, IReadOnlyList<T> items) =>
        new(pageNumber, pageSize, totalCount, (int)Math.Ceiling(totalCount / (double)pageSize), items);
}
