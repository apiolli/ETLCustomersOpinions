using System.Data.Common;

namespace CustomersOpinions.Analytical.Infrastructure.Sources.Database;

public sealed record CustomerRow(string? IdCliente, string? Nombre, string? Email, string? EsInferido)
{
    public static CustomerRow Read(DbDataReader r) => new(
        DbValue.Text(r, 0), DbValue.Text(r, 1), DbValue.Text(r, 2), DbValue.Text(r, 3));
}
