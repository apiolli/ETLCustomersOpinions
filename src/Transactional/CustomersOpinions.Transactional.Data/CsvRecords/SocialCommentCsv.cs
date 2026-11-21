using CsvHelper.Configuration.Attributes;

namespace CustomersOpinions.Transactional.Data.CsvRecords;

/// <summary>Raw row of <c>social_comments.csv</c>.</summary>
public sealed class SocialCommentCsv
{
    [Name("IdComment")]
    public string? CommentId { get; set; }

    [Name("IdCliente")]
    public string? CustomerId { get; set; }

    [Name("IdProducto")]
    public string? ProductId { get; set; }

    [Name("Fuente")]
    public string? Source { get; set; }

    [Name("Fecha")]
    public string? Date { get; set; }

    [Name("Comentario")]
    public string? Comment { get; set; }
}
