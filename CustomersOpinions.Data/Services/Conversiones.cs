using System.Globalization;
using CustomersOpinions.Data.Services.Validacion;

namespace CustomersOpinions.Data.Services;

public static class Conversiones
{
    public static int Entero(string valor) => int.Parse(valor.Trim(), CultureInfo.InvariantCulture);
    public static byte Byte(string valor) => byte.Parse(valor.Trim(), CultureInfo.InvariantCulture);
    public static int QuitarPrefijo(string valor) => Entero(valor.Trim()[1..]);
    public static int? QuitarPrefijoOpcional(string? valor) => Nulo(valor) is { } v ? QuitarPrefijo(v) : null;

    public static DateTime Fecha(string valor) =>
        DateTime.ParseExact(valor.Trim(), Validador.FormatoFecha, CultureInfo.InvariantCulture);

    public static string? Nulo(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
