using System.Configuration;

namespace CustomersOpinions.Transactional.Load;

internal sealed record LoadConfiguration(
    string ConnectionString,
    string CsvFolder,
    string RejectedFolder,
    bool CreateInferredRecords,
    bool DryRun)
{
    public static LoadConfiguration Read(string[] args)
    {
        var dirty = args.Any(a => a.Equals("dirty", StringComparison.OrdinalIgnoreCase));

        var connectionString = ConfigurationManager.ConnectionStrings["OpinionesClientesDB"]?.ConnectionString
            ?? throw new ConfigurationErrorsException("Missing connection string 'OpinionesClientesDB' in App.config.");

        return new LoadConfiguration(
            connectionString,
            Folder(dirty ? "DirtyCsvFolder" : "CsvFolder", dirty ? "CsvFilesDirty" : "CsvFiles"),
            Folder("RejectedFolder", "Rejected"),
            bool.TryParse(ConfigurationManager.AppSettings["CreateInferredRecords"], out var create) && create,
            DryRun: dirty);
    }

    private static string Folder(string key, string defaultValue)
    {
        var folder = ConfigurationManager.AppSettings[key] ?? defaultValue;
        return Path.IsPathRooted(folder) ? folder : Path.Combine(AppContext.BaseDirectory, folder);
    }
}
