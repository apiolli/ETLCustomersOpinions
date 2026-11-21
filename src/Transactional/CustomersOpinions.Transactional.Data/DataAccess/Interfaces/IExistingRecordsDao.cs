using CustomersOpinions.Transactional.Data.Models;

namespace CustomersOpinions.Transactional.Data.DataAccess.Interfaces;

public interface IExistingRecordsDao
{
    IReadOnlyList<string> GetIds(Table table);

    IReadOnlyList<string> GetContentKeys(Table table);
}
