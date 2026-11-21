using System.Data.Common;

namespace CustomersOpinions.Analytical.Infrastructure.Sources.Database;

/// <summary>Row of <c>dbo.vw_ExtractWebReviews</c> (columns read in query order).</summary>
public sealed record WebReviewRow(string? IdReview, string? IdCliente, string? IdProducto, string? Fecha, string? Comentario, string? Rating)
{
    public static WebReviewRow Read(DbDataReader r) => new(
        DbValue.Text(r, 0), DbValue.Text(r, 1), DbValue.Text(r, 2), DbValue.Text(r, 3), DbValue.Text(r, 4), DbValue.Text(r, 5));
}
