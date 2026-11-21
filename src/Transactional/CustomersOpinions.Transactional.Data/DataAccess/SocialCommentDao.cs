using System.Data;
using CustomersOpinions.Transactional.Data.DataAccess.Interfaces;
using CustomersOpinions.Transactional.Data.Models;

namespace CustomersOpinions.Transactional.Data.DataAccess;

public sealed class SocialCommentDao : DaoBase, IInsertDao<SocialComment>
{
    public SocialCommentDao(DbSession session) : base(session) { }

    public bool Insert(SocialComment comment) => ExecuteInsert("dbo.usp_InsertarComentarioSocial", p =>
    {
        p.AddValue("@IdComment", SqlDbType.VarChar, comment.CommentId, 10);
        p.AddValue("@IdCliente", SqlDbType.Int, comment.CustomerId);
        p.AddValue("@IdProducto", SqlDbType.Int, comment.ProductId);
        p.AddValue("@IdRedSocial", SqlDbType.Int, comment.SocialNetworkId);
        p.AddValue("@IdFuente", SqlDbType.VarChar, comment.SourceId, 10);
        p.AddValue("@Fecha", SqlDbType.Date, comment.Date);
        p.AddValue("@Comentario", SqlDbType.VarChar, comment.Comment, 500);
        p.AddValue("@IdClasificacion", SqlDbType.Int, comment.ClassificationId);
    });
}
