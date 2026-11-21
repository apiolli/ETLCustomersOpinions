using System.Data;
using Microsoft.Data.SqlClient;

namespace CustomersOpinions.Data.Daos;

internal static class SqlParameterExtensions
{
    public static void Agregar(this SqlParameterCollection parametros, string nombre, SqlDbType tipo, object? valor, int tamano = 0)
    {
        var parametro = tamano > 0 ? parametros.Add(nombre, tipo, tamano) : parametros.Add(nombre, tipo);
        parametro.Value = valor ?? DBNull.Value;
    }
}
