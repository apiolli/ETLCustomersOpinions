using System.Data.Common;
using CustomersOpinions.Analytical.Domain.Staging;
using CustomersOpinions.Analytical.Infrastructure.Configuration;
using CustomersOpinions.Analytical.Infrastructure.Extraction.Base;
using CustomersOpinions.Analytical.Infrastructure.Persistence;
using CustomersOpinions.Analytical.Infrastructure.Sources.Database;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomersOpinions.Analytical.Infrastructure.Extraction.Database;

public sealed class ProductDatabaseExtractor : DatabaseExtractorBase<StagingProduct>
{
    public ProductDatabaseExtractor(IDbConnectionFactory connections, IOptionsMonitor<DatabaseSourceOptions> options,
        TimeProvider timeProvider, ILogger<ProductDatabaseExtractor> logger)
        : base(connections, options.Get(DatabaseSourceOptions.Products).CommandTimeoutSeconds,
            StagingTableMaps.Products, timeProvider, logger)
    {
    }

    public override string Name => ExtractionQueries.ProductsView;

    protected override string Query => ExtractionQueries.Products;

    protected override StagingProduct Map(DbDataReader reader)
    {
        var row = ProductRow.Read(reader);
        return new StagingProduct
        {
            ProductId = row.IdProducto,
            Name = row.Nombre,
            Category = row.Categoria,
            IsInferred = row.EsInferido
        };
    }
}
