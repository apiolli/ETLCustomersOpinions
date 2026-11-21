namespace CustomersOpinions.Data.Models;

public class FuenteDatos
{
    public string IdFuente { get; set; } = string.Empty;
    public int IdTipoFuente { get; set; }
    public DateTime FechaCarga { get; set; }
}
