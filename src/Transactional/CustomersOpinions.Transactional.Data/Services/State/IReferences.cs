namespace CustomersOpinions.Transactional.Data.Services.State;

public interface IReferences : ILoadState
{
    bool CustomerExists(int customerId);

    bool EmailRegistered(string email);

    void RegisterCustomer(int customerId, string? email);

    bool ProductExists(int productId);

    void RegisterProduct(int productId);

    void RequireCustomer(int customerId);

    void RequireProduct(int productId);
}
