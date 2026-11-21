using System.Data;
using CustomersOpinions.Data.Daos.Interfaces;
using CustomersOpinions.Data.Models;

namespace CustomersOpinions.Data.Daos;

public class ResenaWebDao : DaoBase, IInsertarDao<ResenaWeb>
{
    public ResenaWebDao(DbSession session) : base(session) { }
    public bool Insertar(ResenaWeb resena) => EjecutarInsercion("dbo.usp_InsertarResenaWeb", p =>
    {
        p.Agregar("@IdReview", SqlDbType.VarChar, resena.IdReview, 10);
        p.Agregar("@IdCliente", SqlDbType.Int, resena.IdCliente);
        p.Agregar("@IdProducto", SqlDbType.Int, resena.IdProducto);
        p.Agregar("@IdFuente", SqlDbType.VarChar, resena.IdFuente, 10);
        p.Agregar("@Fecha", SqlDbType.Date, resena.Fecha);
        p.Agregar("@Comentario", SqlDbType.VarChar, resena.Comentario, 500);
        p.Agregar("@Rating", SqlDbType.TinyInt, resena.Rating);
        p.Agregar("@IdClasificacion", SqlDbType.Int, resena.IdClasificacion);
    });
}
