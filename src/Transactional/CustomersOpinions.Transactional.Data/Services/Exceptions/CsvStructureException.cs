namespace CustomersOpinions.Transactional.Data.Services.Exceptions;

/// <summary>The CSV file is missing, empty or lacks required columns; the whole file is rolled back.</summary>
public sealed class CsvStructureException : Exception
{
    public CsvStructureException(string message) : base(message) { }
}
