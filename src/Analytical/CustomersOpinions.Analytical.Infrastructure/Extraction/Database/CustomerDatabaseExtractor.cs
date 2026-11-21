using System.Data.Common;
using CustomersOpinions.Analytical.Domain.Staging;
using CustomersOpinions.Analytical.Infrastructure.Configuration;
using CustomersOpinions.Analytical.Infrastructure.Extraction.Base;
using CustomersOpinions.Analytical.Infrastructure.Persistence;
using CustomersOpinions.Analytical.Infrastructure.Sources.Database;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomersOpinions.Analytical.Infrastructure.Extraction.Database;

public sealed class CustomerDatabaseExtractor : DatabaseExtractorBase<StagingCustomer>
{
    public CustomerDatabaseExtractor(IDbConnectionFactory connections, IOptionsMonitor<DatabaseSourceOptions> options,
        TimeProvider timeProvider, ILogger<CustomerDatabaseExtractor> logger)
        : base(connections, options.Get(DatabaseSourceOptions.Customers).CommandTimeoutSeconds,
            StagingTableMaps.Customers, timeProvider, logger)
    {
    }

    public override string Name => ExtractionQueries.CustomersView;

    protected override string Query => ExtractionQueries.Customers;

    protected override StagingCustomer Map(DbDataReader reader)
    {
        var row = CustomerRow.Read(reader);
        return new StagingCustomer
        {
            CustomerId = row.IdCliente,
            Name = row.Nombre,
            Email = row.Email,
            IsInferred = row.EsInferido
        };
    }
}
