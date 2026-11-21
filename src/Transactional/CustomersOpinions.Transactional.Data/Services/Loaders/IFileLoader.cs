namespace CustomersOpinions.Transactional.Data.Services.Loaders;

public interface IFileLoader
{
    string FileName { get; }

    void Process(string path, LoadResult result);
}
