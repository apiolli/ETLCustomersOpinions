using System.Data;
using CustomersOpinions.Data.Daos.Interfaces;
using CustomersOpinions.Data.Models;

namespace CustomersOpinions.Data.Daos;

public class FuenteDatosDao : DaoBase, IInsertarDao<FuenteDatos>
{
    public FuenteDatosDao(DbSession session) : base(session) { }
    public bool Insertar(FuenteDatos fuente) => EjecutarInsercion("dbo.usp_InsertarFuenteDatos", p =>
    {
        p.Agregar("@IdFuente", SqlDbType.VarChar, fuente.IdFuente, 10);
        p.Agregar("@IdTipoFuente", SqlDbType.Int, fuente.IdTipoFuente);
        p.Agregar("@FechaCarga", SqlDbType.Date, fuente.FechaCarga);
    });
}
