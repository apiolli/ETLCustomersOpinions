using System.Globalization;

namespace CustomersOpinions.Tools.SurveyGenerator;

/// <summary>
/// Deterministic survey rows with the same header as <c>surveys_part1.csv</c>: ids from 100001,
/// customers 1-500, products 1-200, dates 2024-01-01..2025-12-31 and classification consistent with the score.
/// </summary>
internal sealed class SurveyFactory
{
    /// <summary>Header of <c>surveys_part1.csv</c> (data, kept in Spanish).</summary>
    public const string Header = "IdOpinion,IdCliente,IdProducto,Fecha,Comentario,Clasificación,PuntajeSatisfacción,Fuente";

    private const int FirstId = 100_001;
    private const string Source = "EncuestaInterna";
    private static readonly DateTime FirstDate = new(2024, 1, 1);
    private static readonly int DateRangeDays = (new DateTime(2025, 12, 31) - FirstDate).Days + 1;

    // comments are CSV data in Spanish, grouped by classification
    private static readonly string[] Negative =
    {
        "El producto llegó dañado y nadie respondió mi reclamo.",
        "Muy mala calidad, se rompió a la semana de uso.",
        "No cumple con lo que promete la descripción.",
        "La entrega se retrasó demasiado y el empaque venía abierto.",
        "No lo recomiendo, el soporte nunca me ayudó."
    };

    private static readonly string[] Neutral =
    {
        "El producto está bien, pero esperaba un poco más de calidad.",
        "Ni malo ni excelente, simplemente regular.",
        "Cumple su función, aunque el precio es algo elevado.",
        "La entrega fue normal y el producto es aceptable.",
        "Funciona como se espera, sin mayor novedad."
    };

    private static readonly string[] Positive =
    {
        "Excelente producto, superó mis expectativas.",
        "Muy buena calidad y llegó antes de lo previsto.",
        "Lo volvería a comprar, totalmente recomendado.",
        "Gran relación calidad precio, estoy muy satisfecho.",
        "Atención rápida y el producto funciona perfecto."
    };

    private readonly Random _random;

    public SurveyFactory(int seed) => _random = new Random(seed);

    /// <summary>Yields <paramref name="count"/> CSV lines (without the header).</summary>
    public IEnumerable<string> Generate(int count)
    {
        for (var i = 0; i < count; i++)
        {
            var score = _random.Next(1, 6);
            var (classification, comments) = score switch
            {
                <= 2 => ("Negativa", Negative),
                3 => ("Neutra", Neutral),
                _ => ("Positiva", Positive)
            };

            var id = FirstId + i;
            var customer = _random.Next(1, 501);
            var product = _random.Next(1, 201);
            var date = FirstDate.AddDays(_random.Next(DateRangeDays));
            var comment = comments[_random.Next(comments.Length)];

            yield return string.Create(CultureInfo.InvariantCulture,
                $"{id},{customer},{product},{date:yyyy-MM-dd},\"{comment.Replace("\"", "\"\"")}\",{classification},{score},{Source}");
        }
    }
}
