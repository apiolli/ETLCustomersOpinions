using System.Data.Common;

namespace CustomersOpinions.Analytical.Infrastructure.Persistence;

public static class ConnectionNames
{
    public const string TransactionalDb = "TransactionalDb";

    public const string AnalyticalDb = "AnalyticalDb";
}

public interface IDbConnectionFactory
{
    DbConnection Create(string name);
}
