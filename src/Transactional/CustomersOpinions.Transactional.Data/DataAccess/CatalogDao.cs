using System.Data;
using CustomersOpinions.Transactional.Data.DataAccess.Interfaces;
using CustomersOpinions.Transactional.Data.Models;

namespace CustomersOpinions.Transactional.Data.DataAccess;

public sealed class CatalogDao : DaoBase, ICatalogDao
{
    private static readonly Dictionary<Catalog, string> Procedures = new()
    {
        [Catalog.Category] = "dbo.usp_ObtenerOCrearCategoria",
        [Catalog.SourceType] = "dbo.usp_ObtenerOCrearTipoFuente",
        [Catalog.SocialNetwork] = "dbo.usp_ObtenerOCrearRedSocial",
        [Catalog.Classification] = "dbo.usp_ObtenerOCrearClasificacion"
    };

    public CatalogDao(DbSession session) : base(session) { }

    public int GetOrCreate(Catalog catalog, string name)
    {
        using var cmd = CreateCommand(Procedures[catalog]);
        cmd.Parameters.AddValue("@Nombre", SqlDbType.VarChar, name, 50);

        var id = cmd.Parameters.Add("@Id", SqlDbType.Int);
        id.Direction = ParameterDirection.Output;

        cmd.ExecuteNonQuery();
        return (int)id.Value;
    }
}
