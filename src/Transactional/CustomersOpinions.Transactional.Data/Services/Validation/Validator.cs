using System.Globalization;
using System.Text.RegularExpressions;
using CustomersOpinions.Transactional.Data.Services.Exceptions;

namespace CustomersOpinions.Transactional.Data.Services.Validation;

/// <summary>Collects every validation error of a record so the rejection reason lists all of them.</summary>
public sealed class Validator
{
    public const string DateFormat = "yyyy-MM-dd";
    public const int MaxComment = 500;
    public static readonly DateTime MinDate = new(2000, 1, 1);
    private static readonly Regex EmailFormat = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    private readonly List<string> _errors = new();

    public void Add(string error) => _errors.Add(error);

    public void Required(string? value, string field, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Add($"{field} is required");
        }
        else if (value.Length > maxLength)
        {
            Add($"{field} exceeds {maxLength} characters ({value.Length})");
        }
    }

    public void PositiveInteger(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            Add($"{field} is required");
        else if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var n) || n <= 0)
            Add($"{field} must be a positive integer ('{value}')");
    }

    public void Format(string? value, string field, Pattern pattern)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Add($"{field} is required");
        }
        else if (!pattern.Regex.IsMatch(value))
        {
            Add($"{field} has an invalid format '{value}' (expected {pattern.Description})");
        }
    }

    public void OptionalEmail(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return;

        if (value.Length > maxLength)
        {
            Add($"Email exceeds {maxLength} characters");
        }
        else if (!EmailFormat.IsMatch(value))
        {
            Add($"Email has an invalid format '{value}'");
        }
    }

    public void Date(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Add($"{field} is required");
        }
        else if (!DateTime.TryParseExact(value, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            Add($"{field} is invalid '{value}' (expected {DateFormat})");
        }
        else if (date > DateTime.Today)
        {
            Add($"{field} is in the future '{value}'");
        }
        else if (date < MinDate)
        {
            Add($"{field} is too old '{value}' (minimum {MinDate:yyyy-MM-dd})");
        }
    }

    public int? Range(string? value, string field, int min, int max)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Add($"{field} is required");
            return null;
        }
        if (!int.TryParse(value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var n))
        {
            Add($"{field} is not numeric '{value}'");
            return null;
        }
        if (n < min || n > max)
        {
            Add($"{field} out of range {n} (expected {min}-{max})");
            return null;
        }
        return n;
    }

    public void ThrowIfErrors()
    {
        if (_errors.Count > 0)
            throw new ValidationException(_errors);
    }
}
