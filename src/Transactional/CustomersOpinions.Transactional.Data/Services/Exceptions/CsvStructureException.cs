namespace CustomersOpinions.Transactional.Data.Services.Exceptions;

public sealed class CsvStructureException : Exception
{
    public CsvStructureException(string message) : base(message) { }
}
