namespace CustomersOpinions.Data.Models;

public class ComentarioSocial
{
    public string IdComment { get; set; } = string.Empty;
    public int? IdCliente { get; set; }
    public int IdProducto { get; set; }
    public int IdRedSocial { get; set; }
    public string? IdFuente { get; set; }
    public DateTime Fecha { get; set; }
    public string Comentario { get; set; } = string.Empty;
    public int? IdClasificacion { get; set; }
}
