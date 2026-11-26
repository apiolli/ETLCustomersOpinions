using System.Text;
using CustomersOpinions.Analytical.Application;
using CustomersOpinions.Analytical.Infrastructure;
using CustomersOpinions.Analytical.Worker;
using CustomersOpinions.Analytical.Worker.Presentation;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

Console.OutputEncoding = Encoding.UTF8;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService(options => options.ServiceName = "CustomersOpinions ETL");

builder.Services.AddSerilog((_, logger) => logger
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(theme: ConsoleTheme.None, outputTemplate: "[{Timestamp:HH:mm:ss}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(Path.Combine(AppContext.BaseDirectory, "logs", "etl-.log"),
        rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30));

builder.Services.AddOptions<WorkerOptions>()
    .Bind(builder.Configuration.GetSection(WorkerOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<ConsoleReportRenderer>();
builder.Services.AddHostedService<EtlWorker>();

using var host = builder.Build();

var configurationErrors = StartupChecks.Validate(host.Services);
if (configurationErrors.Count > 0)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine("CustomersOpinions ETL cannot start: the configuration is invalid.");
    Console.ResetColor();
    foreach (var error in configurationErrors)
        Console.Error.WriteLine($"  - {error}");
    Console.Error.WriteLine("Secrets are never stored in appsettings.json; configure them with dotnet user-secrets.");
    return 1;
}

await host.RunAsync();
return Environment.ExitCode;
