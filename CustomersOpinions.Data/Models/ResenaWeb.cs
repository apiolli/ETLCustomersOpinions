namespace CustomersOpinions.Data.Models;

public class ResenaWeb
{
    public string IdReview { get; set; } = string.Empty;
    public int IdCliente { get; set; }
    public int IdProducto { get; set; }
    public string? IdFuente { get; set; }
    public DateTime Fecha { get; set; }
    public string Comentario { get; set; } = string.Empty;
    public byte Rating { get; set; }
    public int? IdClasificacion { get; set; }
}
