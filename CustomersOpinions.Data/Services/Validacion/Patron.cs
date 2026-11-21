using System.Text.RegularExpressions;

namespace CustomersOpinions.Data.Services.Validacion;

public sealed record Patron(Regex Regex, string Descripcion)
{
    public static readonly Patron IdCliente = Crear(@"^C\d{3,9}$", "C###");
    public static readonly Patron IdProducto = Crear(@"^P\d{3,9}$", "P###");
    public static readonly Patron IdFuente = Crear(@"^F\d{3,9}$", "F###");
    public static readonly Patron IdComment = Crear(@"^T\d{4,9}$", "T####");
    public static readonly Patron IdReview = Crear(@"^W\d{4,9}$", "W####");

    private static Patron Crear(string expresion, string descripcion) =>
        new(new Regex(expresion, RegexOptions.Compiled | RegexOptions.CultureInvariant), descripcion);
}
