namespace CustomersOpinions.Transactional.Data.Services.State;

public interface IInferredRecords
{
    void EnsureCustomer(int customerId, LoadResult result);

    void EnsureProduct(int productId, LoadResult result);
}
