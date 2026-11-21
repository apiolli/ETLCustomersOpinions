using System.Data;
using Microsoft.Data.SqlClient;

namespace CustomersOpinions.Transactional.Data.DataAccess;

internal static class SqlParameterExtensions
{
    public static void AddValue(this SqlParameterCollection parameters, string name, SqlDbType type, object? value, int size = 0)
    {
        var parameter = size > 0 ? parameters.Add(name, type, size) : parameters.Add(name, type);
        parameter.Value = value ?? DBNull.Value;
    }
}
