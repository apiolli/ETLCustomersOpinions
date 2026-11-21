using System.Data;
using CustomersOpinions.Data.Daos.Interfaces;
using CustomersOpinions.Data.Models;

namespace CustomersOpinions.Data.Daos;

public class ExistentesDao : DaoBase, IExistentesDao
{
    public ExistentesDao(DbSession session) : base(session) { }
    public IReadOnlyList<string> ObtenerIds(Tabla tabla) =>
        LeerTextos("dbo.usp_ObtenerIdsExistentes", tabla);

    public IReadOnlyList<string> ObtenerClavesContenido(Tabla tabla) =>
        LeerTextos("dbo.usp_ObtenerClavesOpiniones", tabla);

    private List<string> LeerTextos(string procedimiento, Tabla tabla) =>
        EjecutarLectura(procedimiento, r => r.GetString(0),
            p => p.Agregar("@Tabla", SqlDbType.VarChar, tabla.ToString(), 30));
}
