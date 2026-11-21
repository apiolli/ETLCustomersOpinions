namespace CustomersOpinions.Transactional.Data.Services.Loaders;

/// <summary>Loads one CSV file into the transactional database.</summary>
public interface IFileLoader
{
    /// <summary>Name of the CSV file this loader reads.</summary>
    string FileName { get; }

    /// <summary>Processes the file at <paramref name="path"/>, accumulating counts and rejections in <paramref name="result"/>.</summary>
    void Process(string path, LoadResult result);
}
