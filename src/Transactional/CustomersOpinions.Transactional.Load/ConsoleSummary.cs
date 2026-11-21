using CustomersOpinions.Transactional.Data.Services;

namespace CustomersOpinions.Transactional.Load;

internal static class ConsoleSummary
{
    private const int MaxRejectionsOnScreen = 60;
    private static readonly string Separator = new('-', 58);

    public static void PrintHeader(LoadConfiguration config)
    {
        Console.WriteLine("CustomersOpinions - OpinionesClientesDB load");
        Console.WriteLine($"Source:                {config.CsvFolder}");
        Console.WriteLine($"Mode:                  {(config.DryRun ? "DRY RUN with dirty CSV files (rolled back at the end)" : "Normal load")}");
        Console.WriteLine($"CreateInferredRecords: {config.CreateInferredRecords}");
        Console.WriteLine();
    }

    public static void Print(LoadSummary summary)
    {
        Console.WriteLine($"{"File",-22}{"Processed",12}{(summary.DryRun ? "Valid" : "Inserted"),12}{"Rejected",12}");
        Console.WriteLine(Separator);
        foreach (var a in summary.Files)
        {
            Console.WriteLine($"{a.FileName,-22}{a.Processed,12}{a.Inserted,12}{a.Rejected,12}");
            if (a.FileError != null)
                PrintError($"  FILE ROLLED BACK: {a.FileError}");
        }
        Console.WriteLine(Separator);
        Console.WriteLine($"{"TOTAL",-22}{summary.Processed,12}{summary.Inserted,12}{summary.Rejected,12}");
        Console.WriteLine();
        Console.WriteLine($"Inferred customers created: {summary.InferredCustomers}");
        Console.WriteLine($"Inferred products created:  {summary.InferredProducts}");
        Console.WriteLine();

        PrintRejections(summary);

        if (summary.DryRun)
            Console.WriteLine("Dry run finished: the transaction was rolled back, the database did not change.");
    }

    public static void PrintError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    private static void PrintRejections(LoadSummary summary)
    {
        if (summary.Rejected == 0) return;

        Console.WriteLine("Rejection reasons:");
        foreach (var r in summary.Rejections.Take(MaxRejectionsOnScreen))
            Console.WriteLine($"  [{r.File} row {r.Row}] {r.RecordId ?? "-"}: {r.Reason}");
        if (summary.Rejected > MaxRejectionsOnScreen)
            Console.WriteLine($"  ... and {summary.Rejected - MaxRejectionsOnScreen} more (see the rejected CSV file)");
        Console.WriteLine();
    }
}
