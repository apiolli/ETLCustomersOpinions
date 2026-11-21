using CustomersOpinions.Transactional.Data.Models;

namespace CustomersOpinions.Transactional.Data.DataAccess.Interfaces;

public interface ICustomerDao : IInsertDao<Customer>
{
    IReadOnlyList<(int CustomerId, string? Email)> GetExisting();
}
