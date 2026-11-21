using CustomersOpinions.Data.Daos.Interfaces;
using CustomersOpinions.Data.Models;

namespace CustomersOpinions.Data.Services.Estado;

public class Provisionales : IProvisionales
{
    public const string NombreSinCategoria = "Sin categoría";
    private readonly IInsertarDao<Cliente> _clienteDao;
    private readonly IInsertarDao<Producto> _productoDao;
    private readonly ICatalogos _catalogos;
    private readonly IReferencias _referencias;
    private readonly bool _habilitado;

    public Provisionales(IInsertarDao<Cliente> clienteDao, IInsertarDao<Producto> productoDao,
        ICatalogos catalogos, IReferencias referencias, bool habilitado)
    {
        _clienteDao = clienteDao;
        _productoDao = productoDao;
        _catalogos = catalogos;
        _referencias = referencias;
        _habilitado = habilitado;
    }

    public void AsegurarCliente(int idCliente, ResultadoCarga resultado)
    {
        if (_referencias.ExisteCliente(idCliente)) return;
        if (!_habilitado)
            _referencias.ExigirCliente(idCliente);

        _clienteDao.Insertar(new Cliente { IdCliente = idCliente, Nombre = $"Cliente_{idCliente}", EsInferido = true });
        _referencias.RegistrarCliente(idCliente, null);
        resultado.ClientesProvisionales++;
    }

    public void AsegurarProducto(int idProducto, ResultadoCarga resultado)
    {
        if (_referencias.ExisteProducto(idProducto)) return;

        if (!_habilitado)
            _referencias.ExigirProducto(idProducto);

        _productoDao.Insertar(new Producto
        {
            IdProducto = idProducto,
            Nombre = $"Producto_{idProducto}",
            IdCategoria = _catalogos.ObtenerId(Catalogo.Categoria, NombreSinCategoria),
            EsInferido = true
        });

        _referencias.RegistrarProducto(idProducto);
        resultado.ProductosProvisionales++;
    }
}
