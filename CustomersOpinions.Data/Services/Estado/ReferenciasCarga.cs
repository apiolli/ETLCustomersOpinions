using CustomersOpinions.Data.Daos.Interfaces;
using CustomersOpinions.Data.Services.Exceptions;

namespace CustomersOpinions.Data.Services.Estado;

public class ReferenciasCarga : IReferencias
{
    private readonly IClienteDao _clienteDao;
    private readonly IProductoDao _productoDao;
    private readonly HashSet<int> _clientes = new();
    private readonly HashSet<string> _emails = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<int> _productos = new();

    public ReferenciasCarga(IClienteDao clienteDao, IProductoDao productoDao)
    {
        _clienteDao = clienteDao;
        _productoDao = productoDao;
    }

    public void Recargar()
    {
        _clientes.Clear();
        _emails.Clear();
        _productos.Clear();
        foreach (var (idCliente, email) in _clienteDao.ObtenerExistentes())
            RegistrarCliente(idCliente, email);
        _productos.UnionWith(_productoDao.ObtenerIdsExistentes());
    }

    public bool ExisteCliente(int idCliente) => _clientes.Contains(idCliente);
    public bool EmailRegistrado(string email) => _emails.Contains(email);
    public void RegistrarCliente(int idCliente, string? email)
    {
        _clientes.Add(idCliente);
        if (email != null)
            _emails.Add(email);
    }

    public bool ExisteProducto(int idProducto) => _productos.Contains(idProducto);
    public void RegistrarProducto(int idProducto) => _productos.Add(idProducto);

    public void ExigirCliente(int idCliente)
    {
        if (!ExisteCliente(idCliente))
            throw new ReferentialIntegrityException($"IdCliente {idCliente} no existe en Cliente");
    }

    public void ExigirProducto(int idProducto)
    {
        if (!ExisteProducto(idProducto))
            throw new ReferentialIntegrityException($"IdProducto {idProducto} no existe en Producto");
    }
}
