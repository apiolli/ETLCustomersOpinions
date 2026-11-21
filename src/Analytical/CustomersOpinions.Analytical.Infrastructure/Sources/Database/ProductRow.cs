using System.Data.Common;

namespace CustomersOpinions.Analytical.Infrastructure.Sources.Database;

/// <summary>Row of <c>dbo.vw_ExtractProducts</c> (columns read in query order).</summary>
public sealed record ProductRow(string? IdProducto, string? Nombre, string? Categoria, string? EsInferido)
{
    public static ProductRow Read(DbDataReader r) => new(
        DbValue.Text(r, 0), DbValue.Text(r, 1), DbValue.Text(r, 2), DbValue.Text(r, 3));
}
