using System.Data;
using CustomersOpinions.Data.Daos.Interfaces;
using CustomersOpinions.Data.Models;

namespace CustomersOpinions.Data.Daos;

public class ProductoDao : DaoBase, IProductoDao
{
    public ProductoDao(DbSession session) : base(session) { }
    public bool Insertar(Producto producto) => EjecutarInsercion("dbo.usp_InsertarProducto", p =>
    {
        p.Agregar("@IdProducto", SqlDbType.Int, producto.IdProducto);
        p.Agregar("@Nombre", SqlDbType.VarChar, producto.Nombre, 100);
        p.Agregar("@IdCategoria", SqlDbType.Int, producto.IdCategoria);
        p.Agregar("@EsInferido", SqlDbType.Bit, producto.EsInferido);
    });

    public IReadOnlyList<int> ObtenerIdsExistentes() =>
        EjecutarLectura("dbo.usp_ObtenerProductosExistentes", r => r.GetInt32(0));
}
