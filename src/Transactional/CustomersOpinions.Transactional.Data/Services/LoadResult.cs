namespace CustomersOpinions.Transactional.Data.Services;

public sealed class LoadResult
{
    public LoadResult(string fileName) => FileName = fileName;

    public string FileName { get; }
    public int Processed { get; set; }
    public int Inserted { get; set; }
    public int InferredCustomers { get; set; }
    public int InferredProducts { get; set; }
    public List<Rejection> Rejections { get; } = new();
    public int Rejected => Rejections.Count;
    public string? FileError { get; set; }

    public void Reject(int row, string? recordId, string reason) =>
        Rejections.Add(new Rejection(FileName, row, recordId, reason));

    public void RollBack(string error)
    {
        FileError = error;
        Inserted = 0;
        InferredCustomers = 0;
        InferredProducts = 0;
    }
}
