using CustomersOpinions.Transactional.Data.Models;

namespace CustomersOpinions.Transactional.Data.DataAccess.Interfaces;

public interface ICatalogDao
{
    int GetOrCreate(Catalog catalog, string name);
}
