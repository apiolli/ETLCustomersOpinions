using CsvHelper.Configuration.Attributes;

namespace CustomersOpinions.Transactional.Data.CsvRecords;

public sealed class WebReviewCsv
{
    [Name("IdReview")]
    public string? ReviewId { get; set; }

    [Name("IdCliente")]
    public string? CustomerId { get; set; }

    [Name("IdProducto")]
    public string? ProductId { get; set; }

    [Name("Fecha")]
    public string? Date { get; set; }

    [Name("Comentario")]
    public string? Comment { get; set; }

    [Name("Rating")]
    public string? Rating { get; set; }
}
