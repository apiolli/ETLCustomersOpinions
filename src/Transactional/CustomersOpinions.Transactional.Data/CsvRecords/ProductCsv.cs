using CsvHelper.Configuration.Attributes;

namespace CustomersOpinions.Transactional.Data.CsvRecords;

public sealed class ProductCsv
{
    [Name("IdProducto")]
    public string? ProductId { get; set; }

    [Name("Nombre")]
    public string? Name { get; set; }

    [Name("Categoría")]
    public string? Category { get; set; }
}
