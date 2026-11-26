using CustomersOpinions.Analytical.Application.Reporting;

namespace CustomersOpinions.Analytical.Application.Abstractions.Loading;

public interface IDimensionLoader
{
    string DimensionName { get; }

    string TableName { get; }

    int Order { get; }

    Task<DimensionLoadResult> LoadAsync(CancellationToken cancellationToken);
}
