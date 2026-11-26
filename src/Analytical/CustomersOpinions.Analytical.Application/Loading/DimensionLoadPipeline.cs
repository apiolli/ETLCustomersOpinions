using System.Diagnostics;
using CustomersOpinions.Analytical.Application.Abstractions.Loading;
using CustomersOpinions.Analytical.Application.Reporting;
using Microsoft.Extensions.Logging;

namespace CustomersOpinions.Analytical.Application.Loading;

public sealed class DimensionLoadPipeline : IDimensionLoadPipeline
{
    private readonly IReadOnlyList<IDimensionLoader> _loaders;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<DimensionLoadPipeline> _logger;

    public DimensionLoadPipeline(IEnumerable<IDimensionLoader> loaders, TimeProvider timeProvider,
        ILogger<DimensionLoadPipeline> logger)
    {
        _loaders = loaders.OrderBy(l => l.Order).ToList();
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<DimensionLoadReport> RunAsync(CancellationToken cancellationToken)
    {
        var startedAt = _timeProvider.GetLocalNow().DateTime;
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Loading {Count} dimensions...", _loaders.Count);

        var results = new List<DimensionLoadResult>(_loaders.Count);
        foreach (var loader in _loaders)
            results.Add(await RunLoaderAsync(loader, cancellationToken));

        stopwatch.Stop();
        var report = new DimensionLoadReport(startedAt, stopwatch.Elapsed, results);
        _logger.LogDebug(
            "Dimension load finished in {Elapsed}: read {Read}, inserted {Inserted}, updated {Updated}, unchanged {Unchanged}, rejected {Rejected}",
            report.Elapsed, report.TotalRead, report.TotalInserted, report.TotalUpdated, report.TotalUnchanged,
            report.TotalRejected);
        return report;
    }

    private async Task<DimensionLoadResult> RunLoaderAsync(IDimensionLoader loader, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await loader.LoadAsync(cancellationToken);
            if (result.Status == SourceStatus.Failed)
                _logger.LogError("{Table} failed: {Message}", loader.TableName, result.Message);
            else
                _logger.LogDebug("{Table}: read {Read}, inserted {Inserted}, updated {Updated}, unchanged {Unchanged}, rejected {Rejected}",
                    loader.TableName, result.Read, result.Inserted, result.Updated, result.Unchanged, result.Rejected);
            return result;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dimension {Dimension} failed with an unexpected error", loader.DimensionName);
            return DimensionLoadResult.Failed(loader.DimensionName, loader.TableName, stopwatch.Elapsed,
                $"Unexpected error: {ex.Message}");
        }
    }
}
