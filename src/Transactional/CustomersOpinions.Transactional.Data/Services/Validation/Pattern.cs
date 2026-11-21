using System.Text.RegularExpressions;

namespace CustomersOpinions.Transactional.Data.Services.Validation;

/// <summary>Expected formats of the prefixed source ids.</summary>
public sealed record Pattern(Regex Regex, string Description)
{
    public static readonly Pattern CustomerId = Create(@"^C\d{3,9}$", "C###");
    public static readonly Pattern ProductId = Create(@"^P\d{3,9}$", "P###");
    public static readonly Pattern SourceId = Create(@"^F\d{3,9}$", "F###");
    public static readonly Pattern CommentId = Create(@"^T\d{4,9}$", "T####");
    public static readonly Pattern ReviewId = Create(@"^W\d{4,9}$", "W####");

    private static Pattern Create(string expression, string description) =>
        new(new Regex(expression, RegexOptions.Compiled | RegexOptions.CultureInvariant), description);
}
