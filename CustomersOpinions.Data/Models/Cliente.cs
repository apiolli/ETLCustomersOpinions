namespace CustomersOpinions.Data.Models;

public class Cliente
{
    public int IdCliente { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool EsInferido { get; set; }
}
