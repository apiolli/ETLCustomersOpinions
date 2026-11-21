using System.Runtime.CompilerServices;
using CustomersOpinions.Analytical.Application.Abstractions.Extraction;
using CustomersOpinions.Analytical.Domain.Common;
using CustomersOpinions.Analytical.Domain.Staging;
using CustomersOpinions.Analytical.Infrastructure.Persistence;
using CustomersOpinions.Analytical.Infrastructure.Sources.Api;
using Microsoft.Extensions.Logging;

namespace CustomersOpinions.Analytical.Infrastructure.Extraction.Base;

/// <summary>
/// Extractor over a paged REST endpoint: reads pages from 1 until <c>pageNumber &gt; totalPages</c>,
/// an empty page or <c>MaxPages</c>. Repeated ids across pages are rejected.
/// A failing page ends the source; records of the previous pages are kept.
/// </summary>
/// <typeparam name="TDto">DTO returned by the API.</typeparam>
/// <typeparam name="TRecord">Staging record type.</typeparam>
public abstract class ApiExtractorBase<TDto, TRecord> : ExtractorBase<TRecord> where TRecord : StagingRecord
{
    private readonly int _maxPages;

    protected ApiExtractorBase(int maxPages, StagingTableMap<TRecord> map, TimeProvider timeProvider, ILogger logger)
        : base(map, timeProvider, logger) => _maxPages = maxPages;

    public override SourceKind SourceKind => SourceKind.Api;

    /// <summary>Gets one page of the endpoint.</summary>
    protected abstract Task<Result<PagedResponse<TDto>>> GetPageAsync(int pageNumber, CancellationToken cancellationToken);

    /// <summary>Source id of a DTO, used to detect duplicates across pages.</summary>
    protected abstract string? KeyOf(TDto dto);

    /// <summary>Converts a DTO into a staging record.</summary>
    protected abstract TRecord Map(TDto dto);

    protected override async IAsyncEnumerable<Result<TRecord>> ExtractCoreAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var pageNumber = 1; pageNumber <= _maxPages; pageNumber++)
        {
            var page = await GetPageAsync(pageNumber, cancellationToken);
            if (page.IsFailure)
            {
                var message = pageNumber == 1 ? page.Message : $"{page.Message} (records of pages 1-{pageNumber - 1} were kept)";
                yield return Result<TRecord>.Failure(ErrorType.SourceUnavailable, message);
                yield break;
            }

            var items = page.Value!.Items ?? Array.Empty<TDto>();
            if (items.Count == 0)
                yield break;

            foreach (var dto in items)
            {
                var key = KeyOf(dto);
                if (string.IsNullOrWhiteSpace(key))
                    yield return Result<TRecord>.Failure(ErrorType.Validation, $"Page {pageNumber}: record without id");
                else if (!seen.Add(key))
                    yield return Result<TRecord>.Failure(ErrorType.Validation, $"Page {pageNumber}: duplicate id {key}");
                else
                    yield return Result<TRecord>.Success(Map(dto));
            }

            if (pageNumber >= page.Value.TotalPages)
                yield break;
        }

        Logger.LogWarning("Source {Source} stopped at the MaxPages limit ({MaxPages})", Name, _maxPages);
    }
}
