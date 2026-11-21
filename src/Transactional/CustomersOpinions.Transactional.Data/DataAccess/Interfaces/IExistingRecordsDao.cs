using CustomersOpinions.Transactional.Data.Models;

namespace CustomersOpinions.Transactional.Data.DataAccess.Interfaces;

/// <summary>Reads what is already stored, to detect duplicates before inserting.</summary>
public interface IExistingRecordsDao
{
    /// <summary>Source ids already stored in the table.</summary>
    IReadOnlyList<string> GetIds(Table table);

    /// <summary>Opinion content keys (customer, product, date and comment) already stored in the table.</summary>
    IReadOnlyList<string> GetContentKeys(Table table);
}
