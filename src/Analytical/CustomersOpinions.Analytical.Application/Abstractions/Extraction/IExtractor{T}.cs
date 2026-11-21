using CustomersOpinions.Analytical.Domain.Common;

namespace CustomersOpinions.Analytical.Application.Abstractions.Extraction;

/// <summary>Streams the records of one source.</summary>
/// <typeparam name="TRecord">Staging record type.</typeparam>
public interface IExtractor<TRecord> : IExtractor
{
    /// <summary>
    /// Yields one result per record: a success with the record, a <see cref="ErrorType.Validation"/>
    /// failure for a bad record, or a single <see cref="ErrorType.SourceUnavailable"/> failure that ends the stream.
    /// </summary>
    IAsyncEnumerable<Result<TRecord>> ExtractAsync(CancellationToken cancellationToken);
}
