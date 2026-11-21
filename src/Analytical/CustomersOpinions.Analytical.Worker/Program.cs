using System.Text;
using CustomersOpinions.Analytical.Application;
using CustomersOpinions.Analytical.Infrastructure;
using CustomersOpinions.Analytical.Worker;
using CustomersOpinions.Analytical.Worker.Presentation;
using Microsoft.Extensions.Options;
using Serilog;

Console.OutputEncoding = Encoding.UTF8;

// configuration order: appsettings.json -> appsettings.{Environment}.json -> User Secrets (Development)
// -> environment variables -> command line
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService(options => options.ServiceName = "CustomersOpinions Extraction");

builder.Services.AddSerilog((_, logger) => logger
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(Path.Combine(AppContext.BaseDirectory, "logs", "extraction-.log"),
        rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30));

builder.Services.AddOptions<WorkerOptions>()
    .Bind(builder.Configuration.GetSection(WorkerOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<ConsoleReportRenderer>();
builder.Services.AddHostedService<ExtractionWorker>();

using var host = builder.Build();

// validate the options before starting, to show clear instructions instead of a stack trace
var configurationErrors = StartupChecks.Validate(host.Services);
if (configurationErrors.Count > 0)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine("CustomersOpinions extraction cannot start: the configuration is invalid.");
    Console.ResetColor();
    foreach (var error in configurationErrors)
        Console.Error.WriteLine($"  - {error}");
    Console.Error.WriteLine("Secrets are never stored in appsettings.json; see src/Analytical/README.md §9.");
    return 1;
}

await host.RunAsync();
return Environment.ExitCode;
