namespace CustomersOpinions.Transactional.Data.Models;

public sealed class DataSource
{
    public string SourceId { get; set; } = string.Empty;
    public int SourceTypeId { get; set; }
    public DateTime LoadDate { get; set; }
}
