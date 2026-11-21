using System.Data;
using CustomersOpinions.Transactional.Data.DataAccess.Interfaces;
using CustomersOpinions.Transactional.Data.Models;

namespace CustomersOpinions.Transactional.Data.DataAccess;

public sealed class CustomerDao : DaoBase, ICustomerDao
{
    public CustomerDao(DbSession session) : base(session) { }

    public bool Insert(Customer customer) => ExecuteInsert("dbo.usp_InsertarCliente", p =>
    {
        p.AddValue("@IdCliente", SqlDbType.Int, customer.CustomerId);
        p.AddValue("@Nombre", SqlDbType.VarChar, customer.Name, 100);
        p.AddValue("@Email", SqlDbType.VarChar, customer.Email, 150);
        p.AddValue("@EsInferido", SqlDbType.Bit, customer.IsInferred);
    });

    public IReadOnlyList<(int CustomerId, string? Email)> GetExisting() =>
        ExecuteRead<(int, string?)>("dbo.usp_ObtenerClientesExistentes",
            r => (r.GetInt32(0), r.IsDBNull(1) ? null : r.GetString(1)));
}
