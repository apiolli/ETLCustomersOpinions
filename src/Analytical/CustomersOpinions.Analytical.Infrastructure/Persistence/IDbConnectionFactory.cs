using System.Data.Common;

namespace CustomersOpinions.Analytical.Infrastructure.Persistence;

/// <summary>Names of the configured connections.</summary>
public static class ConnectionNames
{
    /// <summary><c>OpinionesClientesDB</c>.</summary>
    public const string TransactionalDb = "TransactionalDb";

    /// <summary><c>OpinionesClientesDW</c>.</summary>
    public const string AnalyticalDb = "AnalyticalDb";
}

/// <summary>Creates (unopened) connections by name.</summary>
public interface IDbConnectionFactory
{
    /// <summary>Creates a connection for <paramref name="name"/> (see <see cref="ConnectionNames"/>).</summary>
    DbConnection Create(string name);
}
