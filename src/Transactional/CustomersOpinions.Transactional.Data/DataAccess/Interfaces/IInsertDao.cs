namespace CustomersOpinions.Transactional.Data.DataAccess.Interfaces;

public interface IInsertDao<in T>
{
    bool Insert(T entity);
}
