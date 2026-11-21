using CustomersOpinions.Data.Csvs;
using CustomersOpinions.Data.Daos.Interfaces;
using CustomersOpinions.Data.Models;
using CustomersOpinions.Data.Services.Estado;
using CustomersOpinions.Data.Services.Lectura;
using CustomersOpinions.Data.Services.Validacion;

namespace CustomersOpinions.Data.Services.Cargadores;

public class ComentariosSocialesCargador : CargadorArchivo<ComentarioSocialCsv, ComentarioSocial>
{
    private readonly IInsertarDao<ComentarioSocial> _dao;
    private readonly ICatalogos _catalogos;
    private readonly IReferencias _referencias;
    public ComentariosSocialesCargador(ILectorCsv lector, IExistentesDao existentes, IInsertarDao<ComentarioSocial> dao,
        ICatalogos catalogos, IReferencias referencias) : base(lector, existentes)
    {
        _dao = dao;
        _catalogos = catalogos;
        _referencias = referencias;
    }

    public override string Archivo => "social_comments.csv";
    protected override Tabla Tabla => Tabla.ComentarioSocial;
    protected override string CampoId => "IdComment";
    protected override string? IdOriginal(ComentarioSocialCsv r) => r.IdComment;
    protected override void Validar(ComentarioSocialCsv r, Validador v)
    {
        v.Formato(r.IdComment, "IdComment", Patron.IdComment);
        if (!string.IsNullOrWhiteSpace(r.IdCliente))
        {
            v.Formato(r.IdCliente, "IdCliente", Patron.IdCliente);
        }

        v.Formato(r.IdProducto, "IdProducto", Patron.IdProducto);
        v.Requerido(r.Fuente, "Fuente", 50);
        v.Fecha(r.Fecha, "Fecha");
        v.Requerido(r.Comentario, "Comentario", Validador.MaxComentario);
    }

    protected override ComentarioSocial Transformar(ComentarioSocialCsv r) => new()
    {
        IdComment = r.IdComment!.Trim(),
        IdCliente = Conversiones.QuitarPrefijoOpcional(r.IdCliente),
        IdProducto = Conversiones.QuitarPrefijo(r.IdProducto!),
        IdRedSocial = _catalogos.ObtenerId(Catalogo.RedSocial, r.Fuente!),
        Fecha = Conversiones.Fecha(r.Fecha!),
        Comentario = r.Comentario!.Trim()
    };

    protected override string Id(ComentarioSocial c) => c.IdComment;

    protected override string ClaveContenido(ComentarioSocial c) =>
        ClaveOpinion.Crear(c.IdCliente, c.IdProducto, c.Fecha, c.Comentario);

    protected override bool Insertar(ComentarioSocial comentario, ResultadoCarga resultado)
    {
        if (comentario.IdCliente.HasValue)
            _referencias.ExigirCliente(comentario.IdCliente.Value);
        _referencias.ExigirProducto(comentario.IdProducto);
        return _dao.Insertar(comentario);
    }
}
