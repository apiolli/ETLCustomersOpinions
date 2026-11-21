using System.Data.Common;
using CustomersOpinions.Analytical.Infrastructure.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace CustomersOpinions.Analytical.Infrastructure.Persistence;

/// <summary>SQL Server implementation of <see cref="IDbConnectionFactory"/>.</summary>
public sealed class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly ConnectionStringsOptions _options;

    public SqlConnectionFactory(IOptions<ConnectionStringsOptions> options) => _options = options.Value;

    public DbConnection Create(string name) => name switch
    {
        ConnectionNames.TransactionalDb => new SqlConnection(_options.TransactionalDb),
        ConnectionNames.AnalyticalDb => new SqlConnection(_options.AnalyticalDb),
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown connection name.")
    };
}
