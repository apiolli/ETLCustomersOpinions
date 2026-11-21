using CustomersOpinions.Data.Models;

namespace CustomersOpinions.Data.Daos.Interfaces;

public interface ICatalogoDao
{
    int ObtenerOCrear(Catalogo catalogo, string nombre);
}
