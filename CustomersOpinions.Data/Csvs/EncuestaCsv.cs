using CsvHelper.Configuration.Attributes;

namespace CustomersOpinions.Data.Csvs;

public class EncuestaCsv
{
    public string? IdOpinion { get; set; }
    public string? IdCliente { get; set; }
    public string? IdProducto { get; set; }
    public string? Fecha { get; set; }
    public string? Comentario { get; set; }

    [Name("Clasificación")]
    public string? Clasificacion { get; set; }

    [Name("PuntajeSatisfacción")]
    public string? PuntajeSatisfaccion { get; set; }

    public string? Fuente { get; set; }
}
