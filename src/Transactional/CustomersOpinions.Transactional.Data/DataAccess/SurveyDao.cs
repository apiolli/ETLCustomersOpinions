using System.Data;
using CustomersOpinions.Transactional.Data.DataAccess.Interfaces;
using CustomersOpinions.Transactional.Data.Models;

namespace CustomersOpinions.Transactional.Data.DataAccess;

public sealed class SurveyDao : DaoBase, IInsertDao<Survey>
{
    public SurveyDao(DbSession session) : base(session) { }

    public bool Insert(Survey survey) => ExecuteInsert("dbo.usp_InsertarEncuesta", p =>
    {
        p.AddValue("@IdOpinion", SqlDbType.Int, survey.OpinionId);
        p.AddValue("@IdCliente", SqlDbType.Int, survey.CustomerId);
        p.AddValue("@IdProducto", SqlDbType.Int, survey.ProductId);
        p.AddValue("@IdFuente", SqlDbType.VarChar, survey.SourceId, 10);
        p.AddValue("@Fecha", SqlDbType.Date, survey.Date);
        p.AddValue("@Comentario", SqlDbType.VarChar, survey.Comment, 500);
        p.AddValue("@IdClasificacion", SqlDbType.Int, survey.ClassificationId);
        p.AddValue("@PuntajeSatisfaccion", SqlDbType.TinyInt, survey.SatisfactionScore);
    });
}
