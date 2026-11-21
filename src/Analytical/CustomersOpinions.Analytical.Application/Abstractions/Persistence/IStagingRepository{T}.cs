using CustomersOpinions.Analytical.Domain.Common;

namespace CustomersOpinions.Analytical.Application.Abstractions.Persistence;

/// <summary>Writes the records of one source to its staging table.</summary>
/// <typeparam name="T">Staging record type.</typeparam>
public interface IStagingRepository<T>
{
    /// <summary>Qualified staging table name (e.g. <c>Staging.Surveys</c>).</summary>
    string TableName { get; }

    /// <summary>Empties the staging table.</summary>
    Task<Result> TruncateAsync(CancellationToken cancellationToken);

    /// <summary>Bulk inserts one batch; returns the number of rows written.</summary>
    Task<Result<int>> BulkInsertAsync(IReadOnlyCollection<T> batch, CancellationToken cancellationToken);
}
