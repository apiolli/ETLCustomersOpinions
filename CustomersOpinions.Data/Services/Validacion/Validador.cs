using System.Globalization;
using System.Text.RegularExpressions;
using CustomersOpinions.Data.Services.Exceptions;

namespace CustomersOpinions.Data.Services.Validacion;

public class Validador
{
    public const string FormatoFecha = "yyyy-MM-dd";
    public const int MaxComentario = 500;
    public static readonly DateTime FechaMinima = new(2000, 1, 1);
    private static readonly Regex FormatoEmail = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    private readonly List<string> _errores = new();
    public void Agregar(string error) => _errores.Add(error);

    public void Requerido(string? valor, string campo, int maximo)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            Agregar($"{campo} es obligatorio");
        }
        else if (valor.Length > maximo)
        {
            Agregar($"{campo} supera {maximo} caracteres ({valor.Length})");
        }
    }

    public void EnteroPositivo(string? valor, string campo)
    {
        if (string.IsNullOrWhiteSpace(valor))
            Agregar($"{campo} es obligatorio");
        else if (!int.TryParse(valor, NumberStyles.None, CultureInfo.InvariantCulture, out var n) || n <= 0)
            Agregar($"{campo} debe ser un entero positivo ('{valor}')");
    }

    public void Formato(string? valor, string campo, Patron patron)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            Agregar($"{campo} es obligatorio");
        }
        else if (!patron.Regex.IsMatch(valor))
        {
            Agregar($"{campo} con formato incorrecto '{valor}' (se espera {patron.Descripcion})");
        }
    }

    public void EmailOpcional(string? valor, int maximo)
    {
        if (string.IsNullOrWhiteSpace(valor)) return;

        if (valor.Length > maximo)
        {
            Agregar($"Email supera {maximo} caracteres");
        }
        else if (!FormatoEmail.IsMatch(valor))
        {
            Agregar($"Email con formato inválido '{valor}'");
        }
    }

    public void Fecha(string? valor, string campo)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            Agregar($"{campo} es obligatoria");
        }
        else if (!DateTime.TryParseExact(valor, FormatoFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha))
        {
            Agregar($"{campo} inválida '{valor}' (se espera {FormatoFecha})");
        }
        else if (fecha > DateTime.Today)
        {
            Agregar($"{campo} en el futuro '{valor}'");
        }
        else if (fecha < FechaMinima)
        {
            Agregar($"{campo} demasiado antigua '{valor}' (mínimo {FechaMinima:yyyy-MM-dd})");
        }
    }

    public int? Rango(string? valor, string campo, int min, int max)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            Agregar($"{campo} es obligatorio");
            return null;
        }
        if (!int.TryParse(valor, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var n))
        {
            Agregar($"{campo} no es numérico '{valor}'");
            return null;
        }
        if (n < min || n > max)
        {
            Agregar($"{campo} fuera de rango {n} (se espera {min}-{max})");
            return null;
        }
        return n;
    }

    public void LanzarSiHayErrores()
    {
        if (_errores.Count > 0)
            throw new ValidationException(_errores);
    }
}
