using System.Data;
using CustomersOpinions.Analytical.Domain.DataWarehouse.Dimensions;
using CustomersOpinions.Analytical.Infrastructure.Configuration;
using CustomersOpinions.Analytical.Infrastructure.Loading.Base;
using CustomersOpinions.Analytical.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomersOpinions.Analytical.Infrastructure.Loading;

public sealed class CustomerDimensionLoader : DimensionLoaderBase
{
    private const string Unknown = "Desconocido";

    public CustomerDimensionLoader(IDbConnectionFactory connections, IOptions<DimensionLoadOptions> options,
        ILogger<CustomerDimensionLoader> logger) : base(connections, options, logger)
    {
    }

    public override string DimensionName => "Customer";

    public override string TableName => "dbo.DimCliente";

    public override int Order => 30;

    protected override async Task<LoadCounts> LoadCoreAsync(SqlConnection connection, SqlTransaction transaction,
        CancellationToken cancellationToken)
    {
        var rows = await QueryAsync(connection, transaction, DimensionQueries.StagingCustomers,
            r => (Id: Text(r, 0), Name: Text(r, 1), Email: Text(r, 2), IsInferred: Text(r, 3)), cancellationToken);

        await using var command = Procedure(connection, transaction, DimensionQueries.LoadCustomer);
        var id = command.Parameters.Add("@IdClienteOrigen", SqlDbType.Int);
        var name = command.Parameters.Add("@Nombre", SqlDbType.VarChar, 100);
        var email = command.Parameters.Add("@Email", SqlDbType.VarChar, 150);
        var inferred = command.Parameters.Add("@EsInferido", SqlDbType.Bit);
        var country = command.Parameters.Add("@Pais", SqlDbType.VarChar, 60);
        var ageRange = command.Parameters.Add("@RangoEdad", SqlDbType.VarChar, 20);
        var type = command.Parameters.Add("@TipoCliente", SqlDbType.VarChar, 30);
        var action = command.Parameters.Add(ScdTally.ActionParameter, SqlDbType.VarChar, 12);
        action.Direction = ParameterDirection.Output;

        var tally = new ScdTally();
        var rejected = 0;
        foreach (var row in rows)
        {
            if (!int.TryParse(row.Id, out var sourceId) || sourceId <= 0)
            {
                rejected++;
                Logger.LogWarning("Customer rejected: invalid id {Id}", row.Id);
                continue;
            }

            var customer = new DimCustomer
            {
                SourceCustomerId = sourceId,
                Name = Clip(row.Name, 100, $"Cliente {sourceId}"),
                Email = string.IsNullOrWhiteSpace(row.Email) ? null : Clip(row.Email, 150, string.Empty),
                Country = Unknown,
                AgeRange = Unknown,
                CustomerType = Unknown,
                IsInferred = Flag(row.IsInferred)
            };

            id.Value = customer.SourceCustomerId;
            name.Value = customer.Name;
            email.Value = (object?)customer.Email ?? DBNull.Value;
            inferred.Value = customer.IsInferred;
            country.Value = customer.Country;
            ageRange.Value = customer.AgeRange;
            type.Value = customer.CustomerType;
            await command.ExecuteNonQueryAsync(cancellationToken);
            tally.Add(action.Value);
        }

        return tally.ToCounts(rows.Count, rejected);
    }
}
