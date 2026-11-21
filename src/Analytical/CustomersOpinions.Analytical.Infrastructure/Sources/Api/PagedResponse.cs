namespace CustomersOpinions.Analytical.Infrastructure.Sources.Api;

/// <summary>Paged envelope of the API.</summary>
public sealed record PagedResponse<T>(int PageNumber, int PageSize, int TotalCount, int TotalPages, IReadOnlyList<T>? Items);
