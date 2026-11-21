using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using CustomersOpinions.Transactional.Data.Services.Exceptions;

namespace CustomersOpinions.Transactional.Data.Services.Reading;

/// <summary>CsvHelper based reader: validates the header and yields one <see cref="CsvRow{T}"/> per line.</summary>
public sealed class CsvReaderService : ICsvReader
{
    private static readonly CsvConfiguration Configuration = new(CultureInfo.InvariantCulture)
    {
        TrimOptions = TrimOptions.Trim,
        MissingFieldFound = null
    };

    public IEnumerable<CsvRow<T>> Read<T>(string path)
    {
        if (!File.Exists(path))
            throw new CsvStructureException($"File not found: {path}");

        using var reader = new StreamReader(path, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        using var csv = new CsvReader(reader, Configuration);

        if (!csv.Read())
            throw new CsvStructureException("The file is empty.");
        csv.ReadHeader();
        try
        {
            csv.ValidateHeader<T>();
        }
        catch (HeaderValidationException ex)
        {
            var missing = ex.InvalidHeaders.Select(h => string.Join("/", h.Names));
            throw new CsvStructureException(
                $"Missing columns: {string.Join(", ", missing)}. Header found: {string.Join(",", csv.HeaderRecord!)}");
        }

        var columns = csv.HeaderRecord!.Length;
        while (csv.Read())
        {
            var number = csv.Parser.Row;
            CsvRow<T> row;
            if (csv.Parser.Count != columns)
            {
                row = new CsvRow<T>(number, default, $"The row has {csv.Parser.Count} columns and {columns} were expected");
            }
            else
            {
                try
                {
                    row = new CsvRow<T>(number, csv.GetRecord<T>(), null);
                }
                catch (CsvHelperException ex)
                {
                    row = new CsvRow<T>(number, default, $"Malformed row: {ex.Message.Split('\n')[0]}");
                }
            }
            yield return row;
        }
    }
}
