using System.Diagnostics;
using CustomersOpinions.Analytical.Application.Abstractions.Extraction;
using CustomersOpinions.Analytical.Application.Reporting;
using Microsoft.Extensions.Logging;

namespace CustomersOpinions.Analytical.Application.Extraction;

public sealed class ExtractionPipeline : IExtractionPipeline
{
    private readonly IReadOnlyList<IExtractionSource> _sources;
    private readonly ExtractionSettings _settings;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ExtractionPipeline> _logger;

    public ExtractionPipeline(
        IEnumerable<IExtractionSource> sources,
        ExtractionSettings settings,
        TimeProvider timeProvider,
        ILogger<ExtractionPipeline> logger)
    {
        _sources = sources.ToList();
        _settings = settings;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<ExtractionReport> RunAsync(CancellationToken cancellationToken)
    {
        var startedAt = _timeProvider.GetLocalNow().DateTime;
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Extraction run started with {Count} sources (max parallelism {Parallelism})",
            _sources.Count, _settings.MaxDegreeOfParallelism);

        using var throttle = new SemaphoreSlim(_settings.MaxDegreeOfParallelism);
        var results = await Task.WhenAll(_sources.Select(s => RunSourceAsync(s, throttle, cancellationToken)));

        stopwatch.Stop();
        var report = new ExtractionReport(startedAt, stopwatch.Elapsed, results);
        _logger.LogInformation("Extraction run finished in {Elapsed}: read {Read}, written {Written}, rejected {Rejected}, failed sources {Failed}",
            report.Elapsed, report.TotalRead, report.TotalWritten, report.TotalRejected,
            results.Count(r => r.Status == SourceStatus.Failed));
        return report;
    }

    private async Task<SourceExtractionResult> RunSourceAsync(
        IExtractionSource source, SemaphoreSlim throttle, CancellationToken cancellationToken)
    {
        if (!source.IsEnabled)
        {
            _logger.LogInformation("Source {Source} skipped (disabled)", source.DisplayName);
            return SourceExtractionResult.Skipped(source.DisplayName, source.TableName);
        }

        await throttle.WaitAsync(cancellationToken);
        var stopwatch = Stopwatch.StartNew();
        try
        {
            return await source.RunAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Source {Source} failed with an unexpected error", source.DisplayName);
            return SourceExtractionResult.Failed(source.DisplayName, source.TableName, stopwatch.Elapsed,
                $"Unexpected error: {ex.Message}");
        }
        finally
        {
            throttle.Release();
        }
    }
}
