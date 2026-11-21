using CustomersOpinions.Analytical.Application.Abstractions.Extraction;
using CustomersOpinions.Analytical.Worker.Presentation;
using Microsoft.Extensions.Options;

namespace CustomersOpinions.Analytical.Worker;

/// <summary>
/// Hosted service that runs the extraction pipeline once (exit code 0 = all sources OK, 1 = a source
/// failed, 2 = cancelled) or on a <see cref="PeriodicTimer"/> when installed as a Windows Service.
/// </summary>
public sealed class ExtractionWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ConsoleReportRenderer _renderer;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly WorkerOptions _options;
    private readonly ILogger<ExtractionWorker> _logger;

    public ExtractionWorker(IServiceScopeFactory scopeFactory, ConsoleReportRenderer renderer,
        IHostApplicationLifetime lifetime, IOptions<WorkerOptions> options, ILogger<ExtractionWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _renderer = renderer;
        _lifetime = lifetime;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            if (_options.RunMode == RunMode.Once)
            {
                await RunOnceAsync(stoppingToken);
                return;
            }

            _logger.LogInformation("Interval mode: extraction every {Minutes} minutes", _options.IntervalMinutes);
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_options.IntervalMinutes));
            do
            {
                await RunOnceAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogWarning("Extraction cancelled");
            Environment.ExitCode = 2;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Extraction worker failed");
            Environment.ExitCode = 1;
        }
        finally
        {
            if (_options.RunMode == RunMode.Once)
                _lifetime.StopApplication();
        }
    }

    private async Task RunOnceAsync(CancellationToken stoppingToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var pipeline = scope.ServiceProvider.GetRequiredService<IExtractionPipeline>();
        var report = await pipeline.RunAsync(stoppingToken);
        _renderer.Render(report);
        Environment.ExitCode = report.AnyFailed ? 1 : 0;
    }
}
