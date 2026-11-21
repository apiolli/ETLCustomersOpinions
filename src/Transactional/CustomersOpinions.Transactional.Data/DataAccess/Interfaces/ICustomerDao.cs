using CustomersOpinions.Transactional.Data.Models;

namespace CustomersOpinions.Transactional.Data.DataAccess.Interfaces;

/// <summary>Customer persistence.</summary>
public interface ICustomerDao : IInsertDao<Customer>
{
    /// <summary>Returns the customers already stored, with their e-mail.</summary>
    IReadOnlyList<(int CustomerId, string? Email)> GetExisting();
}
