namespace CustomersOpinions.Transactional.Data.DataAccess.Interfaces;

/// <summary>Inserts one entity.</summary>
/// <typeparam name="T">Entity type.</typeparam>
public interface IInsertDao<in T>
{
    /// <summary>Inserts the entity; returns false when it already existed.</summary>
    bool Insert(T entity);
}
