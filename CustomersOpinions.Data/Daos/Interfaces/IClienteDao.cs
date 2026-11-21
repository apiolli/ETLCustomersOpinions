using CustomersOpinions.Data.Models;

namespace CustomersOpinions.Data.Daos.Interfaces;

public interface IClienteDao : IInsertarDao<Cliente>
{
    IReadOnlyList<(int IdCliente, string? Email)> ObtenerExistentes();
}
