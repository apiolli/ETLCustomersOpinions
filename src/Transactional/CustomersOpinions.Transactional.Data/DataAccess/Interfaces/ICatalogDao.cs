using CustomersOpinions.Transactional.Data.Models;

namespace CustomersOpinions.Transactional.Data.DataAccess.Interfaces;

/// <summary>Resolves catalog ids, creating the entry when it does not exist.</summary>
public interface ICatalogDao
{
    /// <summary>Returns the id of the name in the catalog, creating it if needed.</summary>
    int GetOrCreate(Catalog catalog, string name);
}
