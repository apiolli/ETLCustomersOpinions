using CustomersOpinions.Data.Csvs;
using CustomersOpinions.Data.Daos.Interfaces;
using CustomersOpinions.Data.Models;
using CustomersOpinions.Data.Services.Estado;
using CustomersOpinions.Data.Services.Lectura;
using CustomersOpinions.Data.Services.Validacion;

namespace CustomersOpinions.Data.Services.Cargadores;

public class ProductosCargador : CargadorArchivo<ProductoCsv, Producto>
{
    private readonly IInsertarDao<Producto> _dao;
    private readonly ICatalogos _catalogos;
    private readonly IReferencias _referencias;

    public ProductosCargador(ILectorCsv lector, IExistentesDao existentes, IInsertarDao<Producto> dao,
        ICatalogos catalogos, IReferencias referencias) : base(lector, existentes)
    {
        _dao = dao;
        _catalogos = catalogos;
        _referencias = referencias;
    }

    public override string Archivo => "products.csv";
    protected override Tabla Tabla => Tabla.Producto;
    protected override string CampoId => "IdProducto";
    protected override string? IdOriginal(ProductoCsv r) => r.IdProducto;

    protected override void Validar(ProductoCsv r, Validador v)
    {
        v.EnteroPositivo(r.IdProducto, "IdProducto");
        v.Requerido(r.Nombre, "Nombre", 100);
        v.Requerido(r.Categoria, "Categoría", 50);
    }

    protected override Producto Transformar(ProductoCsv r) => new()
    {
        IdProducto = Conversiones.Entero(r.IdProducto!),
        Nombre = r.Nombre!.Trim(),
        IdCategoria = _catalogos.ObtenerId(Catalogo.Categoria, r.Categoria!)
    };

    protected override string Id(Producto p) => p.IdProducto.ToString();
    protected override bool Insertar(Producto producto, ResultadoCarga resultado)
    {
        if (!_dao.Insertar(producto))
            return false;

        _referencias.RegistrarProducto(producto.IdProducto);
        return true;
    }
}
