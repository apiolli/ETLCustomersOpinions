using CustomersOpinions.Analytical.Domain.Common;

namespace CustomersOpinions.Analytical.Application.Abstractions.Persistence;

public interface IStagingRepository<T>
{
    string TableName { get; }

    Task<Result> TruncateAsync(CancellationToken cancellationToken);

    Task<Result<int>> BulkInsertAsync(IReadOnlyCollection<T> batch, CancellationToken cancellationToken);
}
