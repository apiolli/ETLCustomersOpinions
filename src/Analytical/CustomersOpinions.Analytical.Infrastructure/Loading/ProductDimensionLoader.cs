using System.Data;
using CustomersOpinions.Analytical.Domain.DataWarehouse.Dimensions;
using CustomersOpinions.Analytical.Infrastructure.Configuration;
using CustomersOpinions.Analytical.Infrastructure.Loading.Base;
using CustomersOpinions.Analytical.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomersOpinions.Analytical.Infrastructure.Loading;

public sealed class ProductDimensionLoader : DimensionLoaderBase
{
    private const string NoCategory = "Sin categoría";

    public ProductDimensionLoader(IDbConnectionFactory connections, IOptions<DimensionLoadOptions> options,
        ILogger<ProductDimensionLoader> logger) : base(connections, options, logger)
    {
    }

    public override string DimensionName => "Product";

    public override string TableName => "dbo.DimProducto";

    public override int Order => 40;

    protected override async Task<LoadCounts> LoadCoreAsync(SqlConnection connection, SqlTransaction transaction,
        CancellationToken cancellationToken)
    {
        var rows = await QueryAsync(connection, transaction, DimensionQueries.StagingProducts,
            r => (Id: Text(r, 0), Name: Text(r, 1), Category: Text(r, 2), IsInferred: Text(r, 3)), cancellationToken);

        await using var command = Procedure(connection, transaction, DimensionQueries.LoadProduct);
        var id = command.Parameters.Add("@IdProductoOrigen", SqlDbType.Int);
        var name = command.Parameters.Add("@Nombre", SqlDbType.VarChar, 100);
        var category = command.Parameters.Add("@Categoria", SqlDbType.VarChar, 50);
        var inferred = command.Parameters.Add("@EsInferido", SqlDbType.Bit);
        var action = command.Parameters.Add(ScdTally.ActionParameter, SqlDbType.VarChar, 12);
        action.Direction = ParameterDirection.Output;

        var tally = new ScdTally();
        var rejected = 0;
        foreach (var row in rows)
        {
            if (!int.TryParse(row.Id, out var sourceId) || sourceId <= 0)
            {
                rejected++;
                Logger.LogWarning("Product rejected: invalid id {Id}", row.Id);
                continue;
            }

            var product = new DimProduct
            {
                SourceProductId = sourceId,
                Name = Clip(row.Name, 100, $"Producto {sourceId}"),
                Category = Clip(row.Category, 50, NoCategory),
                IsInferred = Flag(row.IsInferred)
            };

            id.Value = product.SourceProductId;
            name.Value = product.Name;
            category.Value = product.Category;
            inferred.Value = product.IsInferred;
            await command.ExecuteNonQueryAsync(cancellationToken);
            tally.Add(action.Value);
        }

        return tally.ToCounts(rows.Count, rejected);
    }
}
