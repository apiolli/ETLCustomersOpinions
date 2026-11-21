using CustomersOpinions.Transactional.Data.CsvRecords;
using CustomersOpinions.Transactional.Data.DataAccess.Interfaces;
using CustomersOpinions.Transactional.Data.Models;
using CustomersOpinions.Transactional.Data.Services.Reading;
using CustomersOpinions.Transactional.Data.Services.State;
using CustomersOpinions.Transactional.Data.Services.Validation;

namespace CustomersOpinions.Transactional.Data.Services.Loaders;

public sealed class ProductsLoader : FileLoader<ProductCsv, Product>
{
    private readonly IInsertDao<Product> _dao;
    private readonly ICatalogs _catalogs;
    private readonly IReferences _references;

    public ProductsLoader(ICsvReader reader, IExistingRecordsDao existing, IInsertDao<Product> dao,
        ICatalogs catalogs, IReferences references) : base(reader, existing)
    {
        _dao = dao;
        _catalogs = catalogs;
        _references = references;
    }

    public override string FileName => "products.csv";
    protected override Table Table => Table.Product;
    protected override string IdField => "IdProducto";
    protected override string? OriginalId(ProductCsv r) => r.ProductId;

    protected override void Validate(ProductCsv r, Validator v)
    {
        v.PositiveInteger(r.ProductId, "IdProducto");
        v.Required(r.Name, "Nombre", 100);
        v.Required(r.Category, "Categoria", 50);
    }

    protected override Product Transform(ProductCsv r) => new()
    {
        ProductId = Conversions.Integer(r.ProductId!),
        Name = r.Name!.Trim(),
        CategoryId = _catalogs.GetId(Catalog.Category, r.Category!)
    };

    protected override string Id(Product p) => p.ProductId.ToString();

    protected override bool Insert(Product product, LoadResult result)
    {
        if (!_dao.Insert(product))
            return false;

        _references.RegisterProduct(product.ProductId);
        return true;
    }
}
