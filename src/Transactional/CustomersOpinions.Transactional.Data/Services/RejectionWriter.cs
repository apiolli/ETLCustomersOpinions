using System.Globalization;
using System.Text;
using CsvHelper;

namespace CustomersOpinions.Transactional.Data.Services;

/// <summary>Writes the rejected records to <c>rejected_yyyyMMdd_HHmmss.csv</c>.</summary>
public static class RejectionWriter
{
    public static string Save(IEnumerable<Rejection> rejections, string folder)
    {
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, $"rejected_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

        using var writer = new StreamWriter(path, false, new UTF8Encoding(true));
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(rejections);
        return path;
    }
}
