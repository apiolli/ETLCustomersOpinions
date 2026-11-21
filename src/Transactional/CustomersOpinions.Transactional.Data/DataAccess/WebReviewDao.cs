using System.Data;
using CustomersOpinions.Transactional.Data.DataAccess.Interfaces;
using CustomersOpinions.Transactional.Data.Models;

namespace CustomersOpinions.Transactional.Data.DataAccess;

public sealed class WebReviewDao : DaoBase, IInsertDao<WebReview>
{
    public WebReviewDao(DbSession session) : base(session) { }

    public bool Insert(WebReview review) => ExecuteInsert("dbo.usp_InsertarResenaWeb", p =>
    {
        p.AddValue("@IdReview", SqlDbType.VarChar, review.ReviewId, 10);
        p.AddValue("@IdCliente", SqlDbType.Int, review.CustomerId);
        p.AddValue("@IdProducto", SqlDbType.Int, review.ProductId);
        p.AddValue("@IdFuente", SqlDbType.VarChar, review.SourceId, 10);
        p.AddValue("@Fecha", SqlDbType.Date, review.Date);
        p.AddValue("@Comentario", SqlDbType.VarChar, review.Comment, 500);
        p.AddValue("@Rating", SqlDbType.TinyInt, review.Rating);
        p.AddValue("@IdClasificacion", SqlDbType.Int, review.ClassificationId);
    });
}
