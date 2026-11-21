using System.Data;
using CustomersOpinions.Transactional.Data.DataAccess.Interfaces;
using CustomersOpinions.Transactional.Data.Models;

namespace CustomersOpinions.Transactional.Data.DataAccess;

public sealed class DataSourceDao : DaoBase, IInsertDao<DataSource>
{
    public DataSourceDao(DbSession session) : base(session) { }

    public bool Insert(DataSource source) => ExecuteInsert("dbo.usp_InsertarFuenteDatos", p =>
    {
        p.AddValue("@IdFuente", SqlDbType.VarChar, source.SourceId, 10);
        p.AddValue("@IdTipoFuente", SqlDbType.Int, source.SourceTypeId);
        p.AddValue("@FechaCarga", SqlDbType.Date, source.LoadDate);
    });
}
