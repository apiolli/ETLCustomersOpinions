namespace CustomersOpinions.Data.Services.Estado;

public interface IProvisionales
{
    void AsegurarCliente(int idCliente, ResultadoCarga resultado);
    void AsegurarProducto(int idProducto, ResultadoCarga resultado);
}
