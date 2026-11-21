using System.Data;
using CustomersOpinions.Data.Daos.Interfaces;
using CustomersOpinions.Data.Models;

namespace CustomersOpinions.Data.Daos;

public class EncuestaDao : DaoBase, IInsertarDao<Encuesta>
{
    public EncuestaDao(DbSession session) : base(session) { }

    public bool Insertar(Encuesta encuesta) => EjecutarInsercion("dbo.usp_InsertarEncuesta", p =>
    {
        p.Agregar("@IdOpinion", SqlDbType.Int, encuesta.IdOpinion);
        p.Agregar("@IdCliente", SqlDbType.Int, encuesta.IdCliente);
        p.Agregar("@IdProducto", SqlDbType.Int, encuesta.IdProducto);
        p.Agregar("@IdFuente", SqlDbType.VarChar, encuesta.IdFuente, 10);
        p.Agregar("@Fecha", SqlDbType.Date, encuesta.Fecha);
        p.Agregar("@Comentario", SqlDbType.VarChar, encuesta.Comentario, 500);
        p.Agregar("@IdClasificacion", SqlDbType.Int, encuesta.IdClasificacion);
        p.Agregar("@PuntajeSatisfaccion", SqlDbType.TinyInt, encuesta.PuntajeSatisfaccion);
    });
}
