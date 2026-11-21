using CustomersOpinions.Transactional.Data.Models;

namespace CustomersOpinions.Transactional.Data.DataAccess.Interfaces;

public interface IProductDao : IInsertDao<Product>
{
    IReadOnlyList<int> GetExistingIds();
}
