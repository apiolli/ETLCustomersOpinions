using CsvHelper.Configuration.Attributes;

namespace CustomersOpinions.Transactional.Data.CsvRecords;

public sealed class DataSourceCsv
{
    [Name("IdFuente")]
    public string? SourceId { get; set; }

    [Name("TipoFuente")]
    public string? SourceType { get; set; }

    [Name("FechaCarga")]
    public string? LoadDate { get; set; }
}
