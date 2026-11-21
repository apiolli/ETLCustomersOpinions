using System.Data;
using Microsoft.Data.SqlClient;

namespace CustomersOpinions.Transactional.Data.DataAccess;

/// <summary>Base class for DAOs: creates stored procedure commands on the shared session.</summary>
public abstract class DaoBase
{
    private readonly DbSession _session;

    protected DaoBase(DbSession session) => _session = session;

    protected SqlCommand CreateCommand(string procedure) => _session.CreateCommand(procedure);

    /// <summary>Runs an insert procedure that reports through the <c>@Insertado</c> output parameter.</summary>
    protected bool ExecuteInsert(string procedure, Action<SqlParameterCollection> addParameters)
    {
        using var cmd = CreateCommand(procedure);
        addParameters(cmd.Parameters);
        var inserted = cmd.Parameters.Add("@Insertado", SqlDbType.Bit);
        inserted.Direction = ParameterDirection.Output;
        cmd.ExecuteNonQuery();
        return (bool)inserted.Value;
    }

    protected List<T> ExecuteRead<T>(string procedure, Func<SqlDataReader, T> map,
        Action<SqlParameterCollection>? addParameters = null)
    {
        var rows = new List<T>();
        using var cmd = CreateCommand(procedure);
        addParameters?.Invoke(cmd.Parameters);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            rows.Add(map(reader));
        return rows;
    }
}
