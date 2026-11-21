using System.Globalization;

namespace CustomersOpinions.Data.Services;

public static class ClaveOpinion
{
    public static string Crear(int? idCliente, int idProducto, DateTime fecha, string comentario) =>
        string.Create(CultureInfo.InvariantCulture, $"{idCliente}|{idProducto}|{fecha:yyyy-MM-dd}|{comentario}");
}
