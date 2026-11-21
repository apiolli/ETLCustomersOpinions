using System.Data.Common;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using CustomersOpinions.Analytical.Application.Abstractions.Extraction;
using CustomersOpinions.Analytical.Domain.Common;
using CustomersOpinions.Analytical.Domain.Staging;
using CustomersOpinions.Analytical.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using Polly;

namespace CustomersOpinions.Analytical.Infrastructure.Extraction.Base;

public abstract class ExtractorBase<TRecord> : IExtractor<TRecord> where TRecord : StagingRecord
{
    private readonly StagingTableMap<TRecord> _map;
    private readonly TimeProvider _timeProvider;

    protected ExtractorBase(StagingTableMap<TRecord> map, TimeProvider timeProvider, ILogger logger)
    {
        _map = map;
        _timeProvider = timeProvider;
        Logger = logger;
    }

    public abstract string Name { get; }

    public abstract SourceKind SourceKind { get; }

    protected ILogger Logger { get; }

    protected abstract IAsyncEnumerable<Result<TRecord>> ExtractCoreAsync(CancellationToken cancellationToken);

    public async IAsyncEnumerable<Result<TRecord>> ExtractAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var extractedAt = new DateTime(now.Ticks - now.Ticks % TimeSpan.TicksPerSecond, DateTimeKind.Utc);
        var stopwatch = Stopwatch.StartNew();
        var emitted = 0;
        Logger.LogInformation("Extracting {Source} ({Kind})", Name, SourceKind);

        await using var enumerator = ExtractCoreAsync(cancellationToken).GetAsyncEnumerator(cancellationToken);
        while (true)
        {
            Result<TRecord>? current = null;
            string? sourceError = null;
            try
            {
                if (!await enumerator.MoveNextAsync())
                    break;
                current = enumerator.Current;
            }
            catch (Exception ex) when (IsSourceException(ex, cancellationToken))
            {
                sourceError = emitted == 0
                    ? $"{Name} unavailable: {ex.Message}"
                    : $"{Name} failed after {emitted} records: {ex.Message}";
            }

            if (sourceError != null)
            {
                Logger.LogWarning("Source {Source} unavailable after {Elapsed}: {Reason}", Name, stopwatch.Elapsed, sourceError);
                yield return Result<TRecord>.Failure(ErrorType.SourceUnavailable, sourceError);
                yield break;
            }

            if (current!.IsFailure)
            {
                yield return current;
                if (current.ErrorType != ErrorType.Validation)
                    yield break;
                continue;
            }

            var record = (TRecord)current.Value!.WithMetadata(Name, extractedAt);
            var lengths = _map.CheckLengths(record);
            if (lengths.IsFailure)
            {
                yield return Result<TRecord>.Failure(ErrorType.Validation, lengths.Message);
                continue;
            }

            emitted++;
            yield return Result<TRecord>.Success(record);
        }

        stopwatch.Stop();
        Logger.LogInformation("Extracted {Count} records from {Source} in {Elapsed} ({RowsPerSecond:N0} rows/s)",
            emitted, Name, stopwatch.Elapsed,
            stopwatch.Elapsed.TotalSeconds > 0 ? emitted / stopwatch.Elapsed.TotalSeconds : 0);
    }

    private static bool IsSourceException(Exception ex, CancellationToken cancellationToken) => ex switch
    {
        DbException or HttpRequestException or IOException or TimeoutException
            or UnauthorizedAccessException or JsonException or ExecutionRejectedException => true,
        OperationCanceledException => !cancellationToken.IsCancellationRequested,
        _ => false
    };
}
