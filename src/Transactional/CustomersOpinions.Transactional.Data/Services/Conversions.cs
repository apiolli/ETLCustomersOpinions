using System.Globalization;
using CustomersOpinions.Transactional.Data.Services.Validation;

namespace CustomersOpinions.Transactional.Data.Services;

public static class Conversions
{
    public static int Integer(string value) => int.Parse(value.Trim(), CultureInfo.InvariantCulture);

    public static byte Byte(string value) => byte.Parse(value.Trim(), CultureInfo.InvariantCulture);

    public static int StripPrefix(string value) => Integer(value.Trim()[1..]);

    public static int? StripOptionalPrefix(string? value) => NullIfBlank(value) is { } v ? StripPrefix(v) : null;

    public static DateTime Date(string value) =>
        DateTime.ParseExact(value.Trim(), Validator.DateFormat, CultureInfo.InvariantCulture);

    public static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
