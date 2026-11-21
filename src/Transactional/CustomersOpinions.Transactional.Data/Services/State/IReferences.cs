namespace CustomersOpinions.Transactional.Data.Services.State;

/// <summary>Known customers, e-mails and products, used for referential integrity checks.</summary>
public interface IReferences : ILoadState
{
    bool CustomerExists(int customerId);

    bool EmailRegistered(string email);

    void RegisterCustomer(int customerId, string? email);

    bool ProductExists(int productId);

    void RegisterProduct(int productId);

    /// <summary>Throws a referential integrity rejection when the customer does not exist.</summary>
    void RequireCustomer(int customerId);

    /// <summary>Throws a referential integrity rejection when the product does not exist.</summary>
    void RequireProduct(int productId);
}
