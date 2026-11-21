using System.Text;
using CustomersOpinions.Transactional.Data.Services;
using CustomersOpinions.Transactional.Load;

// usage:
//   dotnet run --project src/Transactional/CustomersOpinions.Transactional.Load            -> loads the original CSV files
//   dotnet run --project src/Transactional/CustomersOpinions.Transactional.Load -- dirty   -> validation test (saves nothing)

Console.OutputEncoding = Encoding.UTF8;

try
{
    var config = LoadConfiguration.Read(args);
    ConsoleSummary.PrintHeader(config);

    var summary = LoadFactory.Run(config.ConnectionString, config.CsvFolder, config.DryRun, config.CreateInferredRecords);
    ConsoleSummary.Print(summary);

    if (summary.Rejected > 0)
        Console.WriteLine($"Rejected records detail saved to: {RejectionWriter.Save(summary.Rejections, config.RejectedFolder)}");

    return summary.Files.Any(a => a.FileError != null) ? 2 : 0;
}
catch (Exception ex)
{
    ConsoleSummary.PrintError($"The load could not be run: {ex.Message}");
    return 1;
}
