using System.Configuration;

namespace CustomersOpinions.Load;

internal sealed record ConfiguracionCarga(
    string ConnectionString,
    string CarpetaCsv,
    string CarpetaRechazados,
    bool CrearProvisionales,
    bool Simulacion)
{
    public static ConfiguracionCarga Leer(string[] args)
    {
        var sucios = args.Any(a => a.Equals("sucios", StringComparison.OrdinalIgnoreCase));

        var connectionString = ConfigurationManager.ConnectionStrings["OpinionesClientesDB"]?.ConnectionString
            ?? throw new ConfigurationErrorsException("Falta la cadena de conexión 'OpinionesClientesDB' en App.config.");

        return new ConfiguracionCarga(
            connectionString,
            Carpeta(sucios ? "CarpetaCsvSucios" : "CarpetaCsv", sucios ? "CsvFiles_Sucios" : "CsvFiles"),
            Carpeta("CarpetaRechazados", "Rechazados"),
            bool.TryParse(ConfigurationManager.AppSettings["CrearProvisionales"], out var crear) && crear,
            Simulacion: sucios);
    }

    private static string Carpeta(string clave, string porDefecto)
    {
        var carpeta = ConfigurationManager.AppSettings[clave] ?? porDefecto;
        return Path.IsPathRooted(carpeta) ? carpeta : Path.Combine(AppContext.BaseDirectory, carpeta);
    }
}
