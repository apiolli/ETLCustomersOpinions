namespace CustomersOpinions.Data.Models;

public class Producto
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int IdCategoria { get; set; }
    public bool EsInferido { get; set; }
}
