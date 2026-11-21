using CustomersOpinions.Analytical.Domain.Common;

namespace CustomersOpinions.Analytical.Application.Abstractions.Reading;

/// <summary>Chooses the <see cref="IFileReader{TRow}"/> for a file by its extension (Factory).</summary>
public interface IFileReaderResolver
{
    /// <summary>Returns the reader for <paramref name="path"/>, or a failure when the extension is not supported.</summary>
    Result<IFileReader<TRow>> Resolve<TRow>(string path);
}
