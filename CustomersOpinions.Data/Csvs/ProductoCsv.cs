using CsvHelper.Configuration.Attributes;

namespace CustomersOpinions.Data.Csvs;

public class ProductoCsv
{
    public string? IdProducto { get; set; }
    public string? Nombre { get; set; }

    [Name("Categoría")]
    public string? Categoria { get; set; }
}
