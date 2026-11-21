using CustomersOpinions.Data.Csvs;
using CustomersOpinions.Data.Daos.Interfaces;
using CustomersOpinions.Data.Models;
using CustomersOpinions.Data.Services.Estado;
using CustomersOpinions.Data.Services.Lectura;
using CustomersOpinions.Data.Services.Validacion;

namespace CustomersOpinions.Data.Services.Cargadores;

public class ResenasWebCargador : CargadorArchivo<ResenaCsv, ResenaWeb>
{
    private readonly IInsertarDao<ResenaWeb> _dao;
    private readonly IReferencias _referencias;

    public ResenasWebCargador(ILectorCsv lector, IExistentesDao existentes, IInsertarDao<ResenaWeb> dao,
        IReferencias referencias) : base(lector, existentes)
    {
        _dao = dao;
        _referencias = referencias;
    }

    public override string Archivo => "web_reviews.csv";
    protected override Tabla Tabla => Tabla.ResenaWeb;
    protected override string CampoId => "IdReview";
    protected override string? IdOriginal(ResenaCsv r) => r.IdReview;

    protected override void Validar(ResenaCsv r, Validador v)
    {
        v.Formato(r.IdReview, "IdReview", Patron.IdReview);
        v.Formato(r.IdCliente, "IdCliente", Patron.IdCliente);
        v.Formato(r.IdProducto, "IdProducto", Patron.IdProducto);
        v.Fecha(r.Fecha, "Fecha");
        v.Requerido(r.Comentario, "Comentario", Validador.MaxComentario);
        v.Rango(r.Rating, "Rating", 1, 5);
    }

    protected override ResenaWeb Transformar(ResenaCsv r) => new()
    {
        IdReview = r.IdReview!.Trim(),
        IdCliente = Conversiones.QuitarPrefijo(r.IdCliente!),
        IdProducto = Conversiones.QuitarPrefijo(r.IdProducto!),
        Fecha = Conversiones.Fecha(r.Fecha!),
        Comentario = r.Comentario!.Trim(),
        Rating = Conversiones.Byte(r.Rating!)
    };

    protected override string Id(ResenaWeb w) => w.IdReview;
    protected override string ClaveContenido(ResenaWeb w) =>
        ClaveOpinion.Crear(w.IdCliente, w.IdProducto, w.Fecha, w.Comentario);

    protected override bool Insertar(ResenaWeb resena, ResultadoCarga resultado)
    {
        _referencias.ExigirCliente(resena.IdCliente);
        _referencias.ExigirProducto(resena.IdProducto);
        return _dao.Insertar(resena);
    }
}
