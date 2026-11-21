namespace CustomersOpinions.Transactional.Data.Services;

public sealed class LoadSummary
{
    public bool DryRun { get; init; }
    public List<LoadResult> Files { get; } = new();
    public int Processed => Files.Sum(a => a.Processed);
    public int Inserted => Files.Sum(a => a.Inserted);
    public int Rejected => Files.Sum(a => a.Rejected);
    public int InferredCustomers => Files.Sum(a => a.InferredCustomers);
    public int InferredProducts => Files.Sum(a => a.InferredProducts);
    public IEnumerable<Rejection> Rejections => Files.SelectMany(a => a.Rejections);
}
