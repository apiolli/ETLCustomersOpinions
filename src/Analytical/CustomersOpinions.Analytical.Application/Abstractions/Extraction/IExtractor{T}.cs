using CustomersOpinions.Analytical.Domain.Common;

namespace CustomersOpinions.Analytical.Application.Abstractions.Extraction;

public interface IExtractor<TRecord> : IExtractor
{
    IAsyncEnumerable<Result<TRecord>> ExtractAsync(CancellationToken cancellationToken);
}
