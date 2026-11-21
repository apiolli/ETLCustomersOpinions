using CustomersOpinions.Transactional.Data.Models;

namespace CustomersOpinions.Transactional.Data.Services.State;

/// <summary>Catalog id lookup (get or create).</summary>
public interface ICatalogs : ILoadState
{
    /// <summary>Returns the id of <paramref name="name"/> in <paramref name="catalog"/>.</summary>
    int GetId(Catalog catalog, string name);
}
