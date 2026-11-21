using CustomersOpinions.Transactional.Data.DataAccess.Interfaces;
using CustomersOpinions.Transactional.Data.Models;

namespace CustomersOpinions.Transactional.Data.Services.State;

public sealed class InferredRecords : IInferredRecords
{
    public const string UncategorizedName = "Sin categoría";

    private readonly IInsertDao<Customer> _customerDao;
    private readonly IInsertDao<Product> _productDao;
    private readonly ICatalogs _catalogs;
    private readonly IReferences _references;
    private readonly bool _enabled;

    public InferredRecords(IInsertDao<Customer> customerDao, IInsertDao<Product> productDao,
        ICatalogs catalogs, IReferences references, bool enabled)
    {
        _customerDao = customerDao;
        _productDao = productDao;
        _catalogs = catalogs;
        _references = references;
        _enabled = enabled;
    }

    public void EnsureCustomer(int customerId, LoadResult result)
    {
        if (_references.CustomerExists(customerId)) return;
        if (!_enabled)
            _references.RequireCustomer(customerId);

        _customerDao.Insert(new Customer { CustomerId = customerId, Name = $"Cliente_{customerId}", IsInferred = true });
        _references.RegisterCustomer(customerId, null);
        result.InferredCustomers++;
    }

    public void EnsureProduct(int productId, LoadResult result)
    {
        if (_references.ProductExists(productId)) return;

        if (!_enabled)
            _references.RequireProduct(productId);

        _productDao.Insert(new Product
        {
            ProductId = productId,
            Name = $"Producto_{productId}",
            CategoryId = _catalogs.GetId(Catalog.Category, UncategorizedName),
            IsInferred = true
        });

        _references.RegisterProduct(productId);
        result.InferredProducts++;
    }
}
