using System.Data;
using Microsoft.Data.SqlClient;

namespace CustomersOpinions.Data.Daos;

public abstract class DaoBase
{
    private readonly DbSession _session;
    protected DaoBase(DbSession session) => _session = session;
    protected SqlCommand CrearComando(string procedimiento) => _session.CrearComando(procedimiento);

    protected bool EjecutarInsercion(string procedimiento, Action<SqlParameterCollection> agregarParametros)
    {
        using var cmd = CrearComando(procedimiento);
        agregarParametros(cmd.Parameters);
        var insertado = cmd.Parameters.Add("@Insertado", SqlDbType.Bit);
        insertado.Direction = ParameterDirection.Output;
        cmd.ExecuteNonQuery();
        return (bool)insertado.Value;
    }

    protected List<T> EjecutarLectura<T>(string procedimiento, Func<SqlDataReader, T> mapear,
        Action<SqlParameterCollection>? agregarParametros = null)
    {
        var filas = new List<T>();
        using var cmd = CrearComando(procedimiento);
        agregarParametros?.Invoke(cmd.Parameters);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            filas.Add(mapear(reader));
        return filas;
    }
}
