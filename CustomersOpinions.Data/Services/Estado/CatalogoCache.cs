using CustomersOpinions.Data.Daos.Interfaces;
using CustomersOpinions.Data.Models;

namespace CustomersOpinions.Data.Services.Estado;

public class CatalogoCache : ICatalogos
{
    private readonly ICatalogoDao _dao;
    private readonly Dictionary<Catalogo, Dictionary<string, int>> _ids = new();
    public CatalogoCache(ICatalogoDao dao) => _dao = dao;

    public int ObtenerId(Catalogo catalogo, string nombre)
    {
        nombre = nombre.Trim();
        if (!_ids.TryGetValue(catalogo, out var ids))
            _ids[catalogo] = ids = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        if (!ids.TryGetValue(nombre, out var id))
            ids[nombre] = id = _dao.ObtenerOCrear(catalogo, nombre);
        return id;
    }

    public void Recargar() => _ids.Clear();
}
