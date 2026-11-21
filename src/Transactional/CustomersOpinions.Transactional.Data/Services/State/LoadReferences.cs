using CustomersOpinions.Transactional.Data.DataAccess.Interfaces;
using CustomersOpinions.Transactional.Data.Services.Exceptions;

namespace CustomersOpinions.Transactional.Data.Services.State;

public sealed class LoadReferences : IReferences
{
    private readonly ICustomerDao _customerDao;
    private readonly IProductDao _productDao;
    private readonly HashSet<int> _customers = new();
    private readonly HashSet<string> _emails = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<int> _products = new();

    public LoadReferences(ICustomerDao customerDao, IProductDao productDao)
    {
        _customerDao = customerDao;
        _productDao = productDao;
    }

    public void Reload()
    {
        _customers.Clear();
        _emails.Clear();
        _products.Clear();
        foreach (var (customerId, email) in _customerDao.GetExisting())
            RegisterCustomer(customerId, email);
        _products.UnionWith(_productDao.GetExistingIds());
    }

    public bool CustomerExists(int customerId) => _customers.Contains(customerId);

    public bool EmailRegistered(string email) => _emails.Contains(email);

    public void RegisterCustomer(int customerId, string? email)
    {
        _customers.Add(customerId);
        if (email != null)
            _emails.Add(email);
    }

    public bool ProductExists(int productId) => _products.Contains(productId);

    public void RegisterProduct(int productId) => _products.Add(productId);

    public void RequireCustomer(int customerId)
    {
        if (!CustomerExists(customerId))
            throw new ReferentialIntegrityException($"IdCliente {customerId} does not exist in Cliente");
    }

    public void RequireProduct(int productId)
    {
        if (!ProductExists(productId))
            throw new ReferentialIntegrityException($"IdProducto {productId} does not exist in Producto");
    }
}
