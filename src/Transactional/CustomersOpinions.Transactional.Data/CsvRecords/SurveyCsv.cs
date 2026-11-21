using CsvHelper.Configuration.Attributes;

namespace CustomersOpinions.Transactional.Data.CsvRecords;

/// <summary>Raw row of <c>surveys_part1.csv</c>.</summary>
public sealed class SurveyCsv
{
    [Name("IdOpinion")]
    public string? OpinionId { get; set; }

    [Name("IdCliente")]
    public string? CustomerId { get; set; }

    [Name("IdProducto")]
    public string? ProductId { get; set; }

    [Name("Fecha")]
    public string? Date { get; set; }

    [Name("Comentario")]
    public string? Comment { get; set; }

    [Name("Clasificación")]
    public string? Classification { get; set; }

    [Name("PuntajeSatisfacción")]
    public string? SatisfactionScore { get; set; }

    [Name("Fuente")]
    public string? Source { get; set; }
}
