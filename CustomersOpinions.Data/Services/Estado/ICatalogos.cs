using CustomersOpinions.Data.Models;

namespace CustomersOpinions.Data.Services.Estado;

public interface ICatalogos : IEstadoCarga
{
    int ObtenerId(Catalogo catalogo, string nombre);
}
