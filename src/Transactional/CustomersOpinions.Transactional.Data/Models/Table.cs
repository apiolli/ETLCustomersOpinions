namespace CustomersOpinions.Transactional.Data.Models;

/// <summary>
/// Target tables. Each member maps to the SQL table name sent to the stored procedures,
/// which must not be renamed.
/// </summary>
public enum Table
{
    Customer,
    Product,
    DataSource,
    Survey,
    SocialComment,
    WebReview
}

/// <summary>Maps <see cref="Table"/> members to their SQL table names.</summary>
public static class TableExtensions
{
    /// <summary>Returns the SQL Server table name expected by the stored procedures.</summary>
    public static string ToSqlName(this Table table) => table switch
    {
        Table.Customer => "Cliente",
        Table.Product => "Producto",
        Table.DataSource => "FuenteDatos",
        Table.Survey => "Encuesta",
        Table.SocialComment => "ComentarioSocial",
        Table.WebReview => "ResenaWeb",
        _ => throw new ArgumentOutOfRangeException(nameof(table), table, null)
    };
}
