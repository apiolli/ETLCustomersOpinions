namespace CustomersOpinions.Data.Services.Estado;

public interface IReferencias : IEstadoCarga
{
    bool ExisteCliente(int idCliente);
    bool EmailRegistrado(string email);
    void RegistrarCliente(int idCliente, string? email);
    bool ExisteProducto(int idProducto);
    void RegistrarProducto(int idProducto);
    void ExigirCliente(int idCliente);
    void ExigirProducto(int idProducto);
}
