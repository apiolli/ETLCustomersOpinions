using CustomersOpinions.Data.Models;

namespace CustomersOpinions.Data.Daos.Interfaces;

public interface IExistentesDao
{
    IReadOnlyList<string> ObtenerIds(Tabla tabla);
    IReadOnlyList<string> ObtenerClavesContenido(Tabla tabla);
}
