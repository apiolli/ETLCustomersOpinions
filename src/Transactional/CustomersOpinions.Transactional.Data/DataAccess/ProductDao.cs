using System.Data;
using CustomersOpinions.Transactional.Data.DataAccess.Interfaces;
using CustomersOpinions.Transactional.Data.Models;

namespace CustomersOpinions.Transactional.Data.DataAccess;

public sealed class ProductDao : DaoBase, IProductDao
{
    public ProductDao(DbSession session) : base(session) { }

    public bool Insert(Product product) => ExecuteInsert("dbo.usp_InsertarProducto", p =>
    {
        p.AddValue("@IdProducto", SqlDbType.Int, product.ProductId);
        p.AddValue("@Nombre", SqlDbType.VarChar, product.Name, 100);
        p.AddValue("@IdCategoria", SqlDbType.Int, product.CategoryId);
        p.AddValue("@EsInferido", SqlDbType.Bit, product.IsInferred);
    });

    public IReadOnlyList<int> GetExistingIds() =>
        ExecuteRead("dbo.usp_ObtenerProductosExistentes", r => r.GetInt32(0));
}
