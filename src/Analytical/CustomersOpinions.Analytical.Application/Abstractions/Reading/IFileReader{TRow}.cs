using CustomersOpinions.Analytical.Domain.Common;

namespace CustomersOpinions.Analytical.Application.Abstractions.Reading;

public interface IFileReader<TRow>
{
    IReadOnlyCollection<string> SupportedExtensions { get; }

    IAsyncEnumerable<Result<TRow>> ReadAsync(string path, CancellationToken cancellationToken);
}
