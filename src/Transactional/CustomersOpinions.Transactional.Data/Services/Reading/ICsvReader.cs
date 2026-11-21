namespace CustomersOpinions.Transactional.Data.Services.Reading;

/// <summary>Reads a CSV file as a stream of rows.</summary>
public interface ICsvReader
{
    /// <summary>Reads <paramref name="path"/>; throws <c>CsvStructureException</c> when the header is invalid.</summary>
    IEnumerable<CsvRow<T>> Read<T>(string path);
}
