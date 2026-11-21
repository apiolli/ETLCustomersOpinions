using System.Runtime.CompilerServices;
using CustomersOpinions.Analytical.Application.Abstractions.Extraction;
using CustomersOpinions.Analytical.Application.Abstractions.Reading;
using CustomersOpinions.Analytical.Domain.Common;
using CustomersOpinions.Analytical.Domain.Staging;
using CustomersOpinions.Analytical.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace CustomersOpinions.Analytical.Infrastructure.Extraction.Base;

public abstract class FileExtractorBase<TRow, TRecord> : ExtractorBase<TRecord> where TRecord : StagingRecord
{
    private readonly IFileReaderResolver _resolver;

    protected FileExtractorBase(IFileReaderResolver resolver, string path, StagingTableMap<TRecord> map,
        TimeProvider timeProvider, ILogger logger) : base(map, timeProvider, logger)
    {
        _resolver = resolver;
        FilePath = path;
    }

    protected string FilePath { get; }

    public override string Name => Path.GetFileName(FilePath);

    public override SourceKind SourceKind => SourceKind.File;

    protected abstract TRecord Map(TRow row);

    protected override async IAsyncEnumerable<Result<TRecord>> ExtractCoreAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var reader = _resolver.Resolve<TRow>(FilePath);
        if (reader.IsFailure)
        {
            yield return Result<TRecord>.Failure(reader.ErrorType, reader.Message);
            yield break;
        }

        await foreach (var row in reader.Value!.ReadAsync(FilePath, cancellationToken).WithCancellation(cancellationToken))
        {
            yield return row.IsSuccess
                ? Result<TRecord>.Success(Map(row.Value!))
                : Result<TRecord>.Failure(row.ErrorType, row.Message);
        }
    }
}
