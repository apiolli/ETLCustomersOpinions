using CustomersOpinions.Analytical.Domain.Common;

namespace CustomersOpinions.Analytical.Application.Abstractions.Reading;

/// <summary>Reads one file format (Strategy). A new format such as Excel is a new implementation.</summary>
/// <typeparam name="TRow">Row model with the shape of the file.</typeparam>
public interface IFileReader<TRow>
{
    /// <summary>Extensions handled, including the dot (e.g. ".csv").</summary>
    IReadOnlyCollection<string> SupportedExtensions { get; }

    /// <summary>
    /// Streams the rows. A structure error yields one <see cref="ErrorType.SourceUnavailable"/> failure;
    /// a bad row yields a <see cref="ErrorType.Validation"/> failure.
    /// </summary>
    IAsyncEnumerable<Result<TRow>> ReadAsync(string path, CancellationToken cancellationToken);
}
