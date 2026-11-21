using CsvHelper.Configuration.Attributes;

namespace CustomersOpinions.Transactional.Data.CsvRecords;

/// <summary>Raw row of <c>clients.csv</c>.</summary>
public sealed class CustomerCsv
{
    [Name("IdCliente")]
    public string? CustomerId { get; set; }

    [Name("Nombre")]
    public string? Name { get; set; }

    [Name("Email")]
    public string? Email { get; set; }
}
