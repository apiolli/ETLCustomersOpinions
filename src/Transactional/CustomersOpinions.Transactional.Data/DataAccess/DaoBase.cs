using System.Data;
using Microsoft.Data.SqlClient;

namespace CustomersOpinions.Transactional.Data.DataAccess;

public abstract class DaoBase
{
    private readonly DbSession _session;

    protected DaoBase(DbSession session) => _session = session;

    protected SqlCommand CreateCommand(string procedure) => _session.CreateCommand(procedure);

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
