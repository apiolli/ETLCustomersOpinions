using CustomersOpinions.Transactional.Data.DataAccess.Interfaces;
using CustomersOpinions.Transactional.Data.Models;

namespace CustomersOpinions.Transactional.Data.Services.State;

/// <summary>Caches catalog ids to avoid one database call per record.</summary>
public sealed class CatalogCache : ICatalogs
{
    private readonly ICatalogDao _dao;
    private readonly Dictionary<Catalog, Dictionary<string, int>> _ids = new();

    public CatalogCache(ICatalogDao dao) => _dao = dao;

    public int GetId(Catalog catalog, string name)
    {
        name = name.Trim();
        if (!_ids.TryGetValue(catalog, out var ids))
            _ids[catalog] = ids = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        if (!ids.TryGetValue(name, out var id))
            ids[name] = id = _dao.GetOrCreate(catalog, name);
        return id;
    }

    public void Reload() => _ids.Clear();
}
