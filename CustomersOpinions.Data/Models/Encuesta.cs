namespace CustomersOpinions.Data.Models;

public class Encuesta
{
    public int IdOpinion { get; set; }
    public int IdCliente { get; set; }
    public int IdProducto { get; set; }
    public string? IdFuente { get; set; }
    public DateTime Fecha { get; set; }
    public string Comentario { get; set; } = string.Empty;
    public int IdClasificacion { get; set; }
    public byte PuntajeSatisfaccion { get; set; }
}
