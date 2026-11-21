using System.Data;
using CustomersOpinions.Transactional.Data.DataAccess.Interfaces;
using CustomersOpinions.Transactional.Data.Models;

namespace CustomersOpinions.Transactional.Data.DataAccess;

public sealed class ExistingRecordsDao : DaoBase, IExistingRecordsDao
{
    public ExistingRecordsDao(DbSession session) : base(session) { }

    public IReadOnlyList<string> GetIds(Table table) =>
        ReadStrings("dbo.usp_ObtenerIdsExistentes", table);

    public IReadOnlyList<string> GetContentKeys(Table table) =>
        ReadStrings("dbo.usp_ObtenerClavesOpiniones", table);

    private List<string> ReadStrings(string procedure, Table table) =>
        ExecuteRead(procedure, r => r.GetString(0),
            p => p.AddValue("@Tabla", SqlDbType.VarChar, table.ToSqlName(), 30));
}
