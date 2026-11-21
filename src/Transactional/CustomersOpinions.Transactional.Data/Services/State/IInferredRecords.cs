namespace CustomersOpinions.Transactional.Data.Services.State;

/// <summary>Ensures that the customers and products referenced by a survey exist.</summary>
public interface IInferredRecords
{
    /// <summary>Creates an inferred customer when it does not exist (or rejects when disabled).</summary>
    void EnsureCustomer(int customerId, LoadResult result);

    /// <summary>Creates an inferred product when it does not exist (or rejects when disabled).</summary>
    void EnsureProduct(int productId, LoadResult result);
}
