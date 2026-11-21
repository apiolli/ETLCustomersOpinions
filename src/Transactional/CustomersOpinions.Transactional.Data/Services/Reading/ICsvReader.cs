namespace CustomersOpinions.Transactional.Data.Services.Reading;

public interface ICsvReader
{
    IEnumerable<CsvRow<T>> Read<T>(string path);
}
