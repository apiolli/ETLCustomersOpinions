using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using CustomersOpinions.Data.Services.Exceptions;

namespace CustomersOpinions.Data.Services.Lectura;

public class CsvReaderService : ILectorCsv
{
    private static readonly CsvConfiguration Configuracion = new(CultureInfo.InvariantCulture)
    {
        TrimOptions = TrimOptions.Trim,
        MissingFieldFound = null
    };

    public IEnumerable<FilaCsv<T>> Leer<T>(string ruta)
    {
        if (!File.Exists(ruta))
            throw new EstructuraCsvException($"No se encontró el archivo {ruta}");

        using var reader = new StreamReader(ruta, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        using var csv = new CsvReader(reader, Configuracion);

        if (!csv.Read())
            throw new EstructuraCsvException("El archivo está vacío.");
        csv.ReadHeader();
        try
        {
            csv.ValidateHeader<T>();
        }
        catch (HeaderValidationException ex)
        {
            var faltantes = ex.InvalidHeaders.Select(h => string.Join("/", h.Names));
            throw new EstructuraCsvException(
                $"Faltan columnas: {string.Join(", ", faltantes)}. Cabecera encontrada: {string.Join(",", csv.HeaderRecord!)}");
        }

        var columnas = csv.HeaderRecord!.Length;
        while (csv.Read())
        {
            var numero = csv.Parser.Row;
            FilaCsv<T> fila;
            if (csv.Parser.Count != columnas)
            {
                fila = new FilaCsv<T>(numero, default, $"La fila tiene {csv.Parser.Count} columnas y se esperaban {columnas}");
            }
            else
            {
                try
                {
                    fila = new FilaCsv<T>(numero, csv.GetRecord<T>(), null);
                }
                catch (CsvHelperException ex)
                {
                    fila = new FilaCsv<T>(numero, default, $"Fila mal formada: {ex.Message.Split('\n')[0]}");
                }
            }
            yield return fila;
        }
    }
}
