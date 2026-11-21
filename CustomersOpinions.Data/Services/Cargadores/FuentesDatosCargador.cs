using CustomersOpinions.Data.Csvs;
using CustomersOpinions.Data.Daos.Interfaces;
using CustomersOpinions.Data.Models;
using CustomersOpinions.Data.Services.Estado;
using CustomersOpinions.Data.Services.Lectura;
using CustomersOpinions.Data.Services.Validacion;

namespace CustomersOpinions.Data.Services.Cargadores;

public class FuentesDatosCargador : CargadorArchivo<FuenteDatoCsv, FuenteDatos>
{
    private readonly IInsertarDao<FuenteDatos> _dao;
    private readonly ICatalogos _catalogos;

    public FuentesDatosCargador(ILectorCsv lector, IExistentesDao existentes, IInsertarDao<FuenteDatos> dao,
        ICatalogos catalogos) : base(lector, existentes)
    {
        _dao = dao;
        _catalogos = catalogos;
    }

    public override string Archivo => "fuente_datos.csv";
    protected override Tabla Tabla => Tabla.FuenteDatos;
    protected override string CampoId => "IdFuente";
    protected override string? IdOriginal(FuenteDatoCsv r) => r.IdFuente;
    protected override void Validar(FuenteDatoCsv r, Validador v)
    {
        v.Formato(r.IdFuente, "IdFuente", Patron.IdFuente);
        v.Requerido(r.TipoFuente, "TipoFuente", 50);
        v.Fecha(r.FechaCarga, "FechaCarga");
    }

    protected override FuenteDatos Transformar(FuenteDatoCsv r) => new()
    {
        IdFuente = r.IdFuente!.Trim(),
        IdTipoFuente = _catalogos.ObtenerId(Catalogo.TipoFuente, r.TipoFuente!),
        FechaCarga = Conversiones.Fecha(r.FechaCarga!)
    };

    protected override string Id(FuenteDatos f) => f.IdFuente;
    protected override bool Insertar(FuenteDatos fuente, ResultadoCarga resultado) => _dao.Insertar(fuente);
}
