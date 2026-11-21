namespace CustomersOpinions.Data.Daos.Interfaces;

public interface IInsertarDao<in T>
{
    bool Insertar(T entidad);
}
