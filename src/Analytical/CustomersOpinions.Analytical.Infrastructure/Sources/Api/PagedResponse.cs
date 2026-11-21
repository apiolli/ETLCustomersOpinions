namespace CustomersOpinions.Analytical.Infrastructure.Sources.Api;

public sealed record PagedResponse<T>(int PageNumber, int PageSize, int TotalCount, int TotalPages, IReadOnlyList<T>? Items);
