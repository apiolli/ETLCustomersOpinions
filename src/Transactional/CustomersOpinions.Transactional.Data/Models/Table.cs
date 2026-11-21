namespace CustomersOpinions.Transactional.Data.Models;

public enum Table
{
    Customer,
    Product,
    DataSource,
    Survey,
    SocialComment,
    WebReview
}

public static class TableExtensions
{
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
