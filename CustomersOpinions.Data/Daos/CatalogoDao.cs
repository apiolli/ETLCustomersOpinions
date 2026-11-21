using System.Data;
using CustomersOpinions.Data.Daos.Interfaces;
using CustomersOpinions.Data.Models;

namespace CustomersOpinions.Data.Daos;

public class CatalogoDao : DaoBase, ICatalogoDao
{
    private static readonly Dictionary<Catalogo, string> Procedimientos = new()
    {
        [Catalogo.Categoria] = "dbo.usp_ObtenerOCrearCategoria",
        [Catalogo.TipoFuente] = "dbo.usp_ObtenerOCrearTipoFuente",
        [Catalogo.RedSocial] = "dbo.usp_ObtenerOCrearRedSocial",
        [Catalogo.Clasificacion] = "dbo.usp_ObtenerOCrearClasificacion"
    };

    public CatalogoDao(DbSession session) : base(session) { }

    public int ObtenerOCrear(Catalogo catalogo, string nombre)
    {
        using var cmd = CrearComando(Procedimientos[catalogo]);
        cmd.Parameters.Agregar("@Nombre", SqlDbType.VarChar, nombre, 50);

        var id = cmd.Parameters.Add("@Id", SqlDbType.Int);
        id.Direction = ParameterDirection.Output;

        cmd.ExecuteNonQuery();
        return (int)id.Value;
    }
}
