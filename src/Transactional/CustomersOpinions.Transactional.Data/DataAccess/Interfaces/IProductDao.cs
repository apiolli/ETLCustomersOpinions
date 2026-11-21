using CustomersOpinions.Transactional.Data.Models;

namespace CustomersOpinions.Transactional.Data.DataAccess.Interfaces;

/// <summary>Product persistence.</summary>
public interface IProductDao : IInsertDao<Product>
{
    /// <summary>Returns the ids of the products already stored.</summary>
    IReadOnlyList<int> GetExistingIds();
}
