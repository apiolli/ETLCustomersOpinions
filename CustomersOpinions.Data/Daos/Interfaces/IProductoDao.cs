using CustomersOpinions.Data.Models;

namespace CustomersOpinions.Data.Daos.Interfaces;

public interface IProductoDao : IInsertarDao<Producto>
{
    IReadOnlyList<int> ObtenerIdsExistentes();
}
