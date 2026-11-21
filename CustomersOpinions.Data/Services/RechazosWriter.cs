using System.Globalization;
using System.Text;
using CsvHelper;

namespace CustomersOpinions.Data.Services;

public static class RechazosWriter
{
    public static string Guardar(IEnumerable<Rechazo> rechazos, string carpeta)
    {
        Directory.CreateDirectory(carpeta);
        var ruta = Path.Combine(carpeta, $"rechazados_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

        using var writer = new StreamWriter(ruta, false, new UTF8Encoding(true));
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(rechazos);
        return ruta;
    }
}
