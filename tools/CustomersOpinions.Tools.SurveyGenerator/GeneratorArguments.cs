using System.Globalization;

namespace CustomersOpinions.Tools.SurveyGenerator;

/// <summary>Command line arguments: <c>--count</c> and <c>--output</c>.</summary>
internal sealed record GeneratorArguments(int Count, string Output, string? Error)
{
    private const int DefaultCount = 50_000;
    private const string DefaultOutput = "data/performance/surveys_50k.csv";

    public static GeneratorArguments Parse(string[] args)
    {
        var count = DefaultCount;
        var output = DefaultOutput;

        for (var i = 0; i < args.Length; i++)
        {
            var hasValue = i + 1 < args.Length;
            switch (args[i])
            {
                case "--count" when hasValue:
                    if (!int.TryParse(args[++i], NumberStyles.None, CultureInfo.InvariantCulture, out count) || count < 1)
                        return new GeneratorArguments(0, output, $"Invalid --count '{args[i]}': expected a positive integer.");
                    break;
                case "--output" when hasValue:
                    output = args[++i];
                    break;
                default:
                    return new GeneratorArguments(0, output, $"Unknown or incomplete argument '{args[i]}'.");
            }
        }

        return new GeneratorArguments(count, output, null);
    }
}
