using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace CustomersOpinions.Analytical.Worker;

internal static partial class StartupChecks
{
    public static IReadOnlyList<string> Validate(IServiceProvider services)
    {
        try
        {
            services.GetService<IStartupValidator>()?.Validate();
            return Array.Empty<string>();
        }
        catch (OptionsValidationException ex)
        {
            return Clean(ex.Failures);
        }
        catch (AggregateException ex) when (ex.Flatten().InnerExceptions.All(e => e is OptionsValidationException))
        {
            return Clean(ex.Flatten().InnerExceptions.Cast<OptionsValidationException>().SelectMany(e => e.Failures));
        }
    }

    private static IReadOnlyList<string> Clean(IEnumerable<string> failures) =>
        failures
            .SelectMany(f => f.Split("; ", StringSplitOptions.RemoveEmptyEntries))
            .Select(f => ErrorText().Match(f) is { Success: true } m ? m.Groups[1].Value : f)
            .Distinct()
            .ToList();

    [GeneratedRegex("with the error: '(.*)'\\.?$")]
    private static partial Regex ErrorText();
}
