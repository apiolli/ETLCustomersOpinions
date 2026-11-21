using System.Globalization;
using System.Text;
using CustomersOpinions.Tools.SurveyGenerator;

var arguments = GeneratorArguments.Parse(args);
if (arguments.Error != null)
{
    Console.Error.WriteLine(arguments.Error);
    Console.Error.WriteLine("Usage: --count <rows> (default 50000) --output <path> (default data/performance/surveys_50k.csv)");
    return 1;
}

var outputPath = Path.GetFullPath(arguments.Output);
Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

var stopwatch = System.Diagnostics.Stopwatch.StartNew();
await using (var writer = new StreamWriter(outputPath, append: false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
{
    await writer.WriteLineAsync(SurveyFactory.Header);
    foreach (var line in new SurveyFactory(seed: 42).Generate(arguments.Count))
        await writer.WriteLineAsync(line);
}

Console.WriteLine(string.Create(CultureInfo.InvariantCulture,
    $"Generated {arguments.Count:N0} surveys in {stopwatch.Elapsed.TotalSeconds:N2} s -> {outputPath}"));
return 0;
