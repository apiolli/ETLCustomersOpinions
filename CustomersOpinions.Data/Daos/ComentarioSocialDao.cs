using System.Data;
using CustomersOpinions.Data.Daos.Interfaces;
using CustomersOpinions.Data.Models;

namespace CustomersOpinions.Data.Daos;

public class ComentarioSocialDao : DaoBase, IInsertarDao<ComentarioSocial>
{
    public ComentarioSocialDao(DbSession session) : base(session) { }

    public bool Insertar(ComentarioSocial comentario) => EjecutarInsercion("dbo.usp_InsertarComentarioSocial", p =>
    {
        p.Agregar("@IdComment", SqlDbType.VarChar, comentario.IdComment, 10);
        p.Agregar("@IdCliente", SqlDbType.Int, comentario.IdCliente);
        p.Agregar("@IdProducto", SqlDbType.Int, comentario.IdProducto);
        p.Agregar("@IdRedSocial", SqlDbType.Int, comentario.IdRedSocial);
        p.Agregar("@IdFuente", SqlDbType.VarChar, comentario.IdFuente, 10);
        p.Agregar("@Fecha", SqlDbType.Date, comentario.Fecha);
        p.Agregar("@Comentario", SqlDbType.VarChar, comentario.Comentario, 500);
        p.Agregar("@IdClasificacion", SqlDbType.Int, comentario.IdClasificacion);
    });
}
