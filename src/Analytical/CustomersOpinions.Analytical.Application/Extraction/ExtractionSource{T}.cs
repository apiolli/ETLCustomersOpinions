using System.Diagnostics;
using CustomersOpinions.Analytical.Application.Abstractions.Extraction;
using CustomersOpinions.Analytical.Application.Abstractions.Persistence;
using CustomersOpinions.Analytical.Application.Reporting;
using CustomersOpinions.Analytical.Domain.Common;
using Microsoft.Extensions.Logging;

namespace CustomersOpinions.Analytical.Application.Extraction;

/// <summary>
/// Runs one extractor into its staging table: full refresh, truncating only when the first
/// valid record arrives, bounded batches and per source metrics.
/// </summary>
/// <typeparam name="T">Staging record type.</typeparam>
public sealed class ExtractionSource<T> : IExtractionSource
{
    private const int RejectionsLoggedOneByOne = 20;

    private readonly IExtractor<T> _extractor;
    private readonly IStagingRepository<T> _repository;
    private readonly SourceSettings _settings;
    private readonly ExtractionSettings _extraction;
    private readonly ILogger<ExtractionSource<T>> _logger;

    public ExtractionSource(
        IExtractor<T> extractor,
        IStagingRepository<T> repository,
        SourceSettings settings,
        ExtractionSettings extraction,
        ILogger<ExtractionSource<T>> logger)
    {
        _extractor = extractor;
        _repository = repository;
        _settings = settings;
        _extraction = extraction;
        _logger = logger;
    }

    public string DisplayName => _settings.DisplayName;

    public string TableName => _repository.TableName;

    public bool IsEnabled => _settings.Enabled;

    public async Task<SourceExtractionResult> RunAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var buffer = new List<T>(_extraction.BatchSize);
        var read = 0;
        var written = 0;
        var rejected = 0;
        var truncated = false;
        string? failure = null;

        _logger.LogInformation("Source {Source} started -> {Table}", DisplayName, TableName);

        await foreach (var result in _extractor.ExtractAsync(cancellationToken).WithCancellation(cancellationToken))
        {
            if (result.IsSuccess)
            {
                read++;
                if (!truncated)
                {
                    var truncate = await _repository.TruncateAsync(cancellationToken);
                    if (truncate.IsFailure)
                    {
                        failure = truncate.Message;
                        break;
                    }
                    truncated = true;
                }

                buffer.Add(result.Value!);
                if (buffer.Count >= _extraction.BatchSize)
                {
                    var flush = await FlushAsync(buffer, cancellationToken);
                    if (flush.IsFailure)
                    {
                        failure = flush.Message;
                        break;
                    }
                    written += flush.Value;
                }
            }
            else if (result.ErrorType == ErrorType.Validation)
            {
                read++;
                rejected++;
                if (rejected <= RejectionsLoggedOneByOne)
                    _logger.LogWarning("Source {Source}: record rejected: {Reason}", DisplayName, result.Message);
            }
            else
            {
                failure = result.Message;
                break;
            }
        }

        // keep what was already extracted (e.g. an API page fails after the first ones)
        if (buffer.Count > 0 && truncated)
        {
            var flush = await FlushAsync(buffer, cancellationToken);
            if (flush.IsSuccess)
                written += flush.Value;
            else
                failure ??= flush.Message;
        }

        // a source that returned no valid rows still replaces the previous content
        if (failure == null && !truncated)
        {
            var truncate = await _repository.TruncateAsync(cancellationToken);
            if (truncate.IsFailure)
                failure = truncate.Message;
        }

        stopwatch.Stop();

        if (rejected > RejectionsLoggedOneByOne)
            _logger.LogWarning("Source {Source}: {Count} more rejected records not logged individually",
                DisplayName, rejected - RejectionsLoggedOneByOne);

        var status = failure != null ? SourceStatus.Failed
            : rejected > 0 ? SourceStatus.CompletedWithRejections
            : SourceStatus.Succeeded;

        var report = new SourceExtractionResult(DisplayName, TableName, status, read, written, rejected,
            stopwatch.Elapsed, failure ?? string.Empty);

        if (failure != null)
            _logger.LogError("Source {Source} failed after {Elapsed}: {Reason}", DisplayName, stopwatch.Elapsed, failure);
        else
            _logger.LogInformation(
                "Source {Source} finished: read {Read}, written {Written}, rejected {Rejected} in {Elapsed} ({RowsPerSecond:N0} rows/s)",
                DisplayName, read, written, rejected, stopwatch.Elapsed, report.RowsPerSecond);

        return report;
    }

    private async Task<Result<int>> FlushAsync(List<T> buffer, CancellationToken cancellationToken)
    {
        var result = await _repository.BulkInsertAsync(buffer.ToArray(), cancellationToken);
        buffer.Clear();
        return result;
    }
}
