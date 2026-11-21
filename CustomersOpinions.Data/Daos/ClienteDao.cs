using System.Data;
using CustomersOpinions.Data.Daos.Interfaces;
using CustomersOpinions.Data.Models;

namespace CustomersOpinions.Data.Daos;

public class ClienteDao : DaoBase, IClienteDao
{
    public ClienteDao(DbSession session) : base(session) { }

    public bool Insertar(Cliente cliente) => EjecutarInsercion("dbo.usp_InsertarCliente", p =>
    {
        p.Agregar("@IdCliente", SqlDbType.Int, cliente.IdCliente);
        p.Agregar("@Nombre", SqlDbType.VarChar, cliente.Nombre, 100);
        p.Agregar("@Email", SqlDbType.VarChar, cliente.Email, 150);
        p.Agregar("@EsInferido", SqlDbType.Bit, cliente.EsInferido);
    });

    public IReadOnlyList<(int IdCliente, string? Email)> ObtenerExistentes() =>
        EjecutarLectura<(int, string?)>("dbo.usp_ObtenerClientesExistentes",
            r => (r.GetInt32(0), r.IsDBNull(1) ? null : r.GetString(1)));
}
