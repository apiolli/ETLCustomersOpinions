using System.Data.Common;
using System.Globalization;

namespace CustomersOpinions.Analytical.Infrastructure.Sources.Database;

internal static class DbValue
{
    public static string? Text(DbDataReader reader, int ordinal)
    {
        if (reader.IsDBNull(ordinal))
            return null;

        return reader.GetValue(ordinal) switch
        {
            DateTime date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            bool flag => flag ? "1" : "0",
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            var value => value.ToString()
        };
    }
}
