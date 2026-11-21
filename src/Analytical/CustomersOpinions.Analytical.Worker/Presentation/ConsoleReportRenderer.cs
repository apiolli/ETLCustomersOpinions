using System.Globalization;
using CustomersOpinions.Analytical.Application.Reporting;

namespace CustomersOpinions.Analytical.Worker.Presentation;

/// <summary>Prints the extraction report as a table on the console.</summary>
public sealed class ConsoleReportRenderer
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    public void Render(ExtractionReport report)
    {
        Console.WriteLine();
        Console.WriteLine($"CustomersOpinions — Extraction run {report.StartedAt.ToString("yyyy-MM-dd HH:mm:ss", Culture)}");
        Console.WriteLine(Row("Source", "Staging table", "Status", "Read", "Written", "Rejected", "Elapsed", "Rows/s", ""));

        foreach (var r in report.Results)
        {
            var line = Row(r.SourceName, r.TableName, Label(r.Status),
                Number(r.Read), Number(r.Written), Number(r.Rejected), Time(r.Elapsed),
                Number((int)Math.Round(r.RowsPerSecond)), r.Message);

            if (r.Status == SourceStatus.Failed)
                WriteColored(line, ConsoleColor.Red);
            else if (r.Status == SourceStatus.CompletedWithRejections)
                WriteColored(line, ConsoleColor.Yellow);
            else
                Console.WriteLine(line);
        }

        Console.WriteLine(Row("TOTAL", "", "", Number(report.TotalRead), Number(report.TotalWritten),
            Number(report.TotalRejected), Time(report.Elapsed), "", ""));
        Console.WriteLine();
    }

    private static string Row(string source, string table, string status, string read, string written,
        string rejected, string elapsed, string rowsPerSecond, string message) =>
        $"{source,-22}{table,-27}{status,-10}{read,8}{written,10}{rejected,10}{elapsed,10}{rowsPerSecond,9}   {message}".TrimEnd();

    private static string Label(SourceStatus status) => status switch
    {
        SourceStatus.Succeeded => "OK",
        SourceStatus.CompletedWithRejections => "OK*",
        SourceStatus.Failed => "FAILED",
        _ => "SKIPPED"
    };

    private static string Number(int value) => value.ToString("N0", Culture);

    private static string Time(TimeSpan elapsed) => elapsed.ToString(@"mm\:ss\.ff", Culture);

    private static void WriteColored(string line, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(line);
        Console.ResetColor();
    }
}
