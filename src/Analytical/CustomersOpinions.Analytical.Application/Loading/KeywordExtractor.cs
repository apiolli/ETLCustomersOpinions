using System.Text.RegularExpressions;

namespace CustomersOpinions.Analytical.Application.Loading;

public static partial class KeywordExtractor
{
    public const int MaxWordLength = 50;

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "algo", "algún", "alguna", "algunas", "alguno", "algunos", "ante", "antes", "aquel", "aquella", "aquí",
        "bastante", "bien", "cada", "casi", "como", "cómo", "con", "contra", "cual", "cuál", "cuando", "cuándo",
        "desde", "después", "donde", "dónde", "durante", "ella", "ellas", "ellos", "entre", "era", "eran", "eres",
        "esa", "esas", "ese", "eso", "esos", "esta", "está", "estaba", "están", "estar", "estas", "este", "esto",
        "estos", "estoy", "fue", "fueron", "hace", "hacer", "hasta", "hay", "luego", "más", "mientras", "mismo",
        "mucho", "muchos", "muy", "nada", "nadie", "ninguna", "ninguno", "nosotros", "nuestra", "nuestro", "otra",
        "otras", "otro", "otros", "para", "pero", "poco", "por", "porque", "pues", "puede", "qué", "que", "quien",
        "sea", "según", "sido", "siempre", "sin", "sino", "sobre", "solo", "sólo", "son", "sus", "también", "tan",
        "tanto", "tenemos", "tener", "tengo", "tiene", "tienen", "todo", "todos", "una", "unas", "uno", "unos",
        "usted", "ustedes", "vez"
    };

    public static IEnumerable<string> Extract(string? text, int minLength)
    {
        if (string.IsNullOrWhiteSpace(text))
            yield break;

        foreach (Match match in Word().Matches(text))
        {
            var word = match.Value.ToLowerInvariant();
            if (word.Length >= minLength && word.Length <= MaxWordLength && !StopWords.Contains(word))
                yield return word;
        }
    }

    [GeneratedRegex(@"\p{L}+")]
    private static partial Regex Word();
}
