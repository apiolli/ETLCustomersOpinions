using CustomersOpinions.Analytical.Application.Abstractions.Extraction;
using CustomersOpinions.Analytical.Application.Abstractions.Loading;
using CustomersOpinions.Analytical.Worker.Presentation;
using Microsoft.Extensions.Options;

namespace CustomersOpinions.Analytical.Worker;

public sealed class EtlWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ConsoleReportRenderer _renderer;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly WorkerOptions _options;
    private readonly ILogger<EtlWorker> _logger;

    public EtlWorker(IServiceScopeFactory scopeFactory, ConsoleReportRenderer renderer,
        IHostApplicationLifetime lifetime, IOptions<WorkerOptions> options, ILogger<EtlWorker> logger)
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

            _logger.LogInformation("Interval mode: stage {Stage} every {Minutes} minutes", _options.Stage, _options.IntervalMinutes);
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_options.IntervalMinutes));
            do
            {
                await RunOnceAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogWarning("ETL run cancelled");
            Environment.ExitCode = 2;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ETL worker failed");
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
        var failed = false;

        if (_options.Stage is EtlStage.All or EtlStage.Extraction)
        {
            var extraction = await scope.ServiceProvider.GetRequiredService<IExtractionPipeline>().RunAsync(stoppingToken);
            _renderer.Render(extraction);
            failed = extraction.AnyFailed;
        }

        if (_options.Stage is EtlStage.All or EtlStage.Dimensions)
        {
            if (failed)
            {
                _logger.LogWarning("Dimension load skipped because the extraction failed; staging may be incomplete");
            }
            else
            {
                var dimensions = await scope.ServiceProvider.GetRequiredService<IDimensionLoadPipeline>().RunAsync(stoppingToken);
                _renderer.Render(dimensions);
                failed = dimensions.AnyFailed;
            }
        }

        Environment.ExitCode = failed ? 1 : 0;
    }
}
