using CustomersOpinions.Data.Csvs;
using CustomersOpinions.Data.Daos.Interfaces;
using CustomersOpinions.Data.Models;
using CustomersOpinions.Data.Services.Estado;
using CustomersOpinions.Data.Services.Lectura;
using CustomersOpinions.Data.Services.Validacion;

namespace CustomersOpinions.Data.Services.Cargadores;

public class EncuestasCargador : CargadorArchivo<EncuestaCsv, Encuesta>
{
    private static readonly Dictionary<string, (int Min, int Max)> RangoPorClasificacion =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Negativa"] = (1, 2),
            ["Neutra"] = (3, 3),
            ["Positiva"] = (4, 5)
        };

    private readonly IInsertarDao<Encuesta> _dao;
    private readonly ICatalogos _catalogos;
    private readonly IProvisionales _provisionales;

    public EncuestasCargador(ILectorCsv lector, IExistentesDao existentes, IInsertarDao<Encuesta> dao,
        ICatalogos catalogos, IProvisionales provisionales) : base(lector, existentes)
    {
        _dao = dao;
        _catalogos = catalogos;
        _provisionales = provisionales;
    }

    public override string Archivo => "surveys_part1.csv";
    protected override Tabla Tabla => Tabla.Encuesta;
    protected override string CampoId => "IdOpinion";
    protected override string? IdOriginal(EncuestaCsv r) => r.IdOpinion;

    protected override void Validar(EncuestaCsv r, Validador v)
    {
        v.EnteroPositivo(r.IdOpinion, "IdOpinion");
        v.EnteroPositivo(r.IdCliente, "IdCliente");
        v.EnteroPositivo(r.IdProducto, "IdProducto");
        v.Fecha(r.Fecha, "Fecha");
        v.Requerido(r.Comentario, "Comentario", Validador.MaxComentario);
        v.Requerido(r.Fuente, "Fuente", 50);
        var puntaje = v.Rango(r.PuntajeSatisfaccion, "PuntajeSatisfacción", 1, 5);

        if (string.IsNullOrWhiteSpace(r.Clasificacion))
        {
            v.Agregar("Clasificación es obligatoria");

        }
        else if (!RangoPorClasificacion.TryGetValue(r.Clasificacion, out var rango))
        {
            v.Agregar($"Clasificación desconocida '{r.Clasificacion}' (Positiva, Neutra o Negativa)");

        }
        else if (puntaje.HasValue && (puntaje < rango.Min || puntaje > rango.Max))
        {
            v.Agregar($"Clasificación '{r.Clasificacion}' incoherente con el puntaje {puntaje}");

        }
    }

    protected override Encuesta Transformar(EncuestaCsv r) => new()
    {
        IdOpinion = Conversiones.Entero(r.IdOpinion!),
        IdCliente = Conversiones.Entero(r.IdCliente!),
        IdProducto = Conversiones.Entero(r.IdProducto!),
        Fecha = Conversiones.Fecha(r.Fecha!),
        Comentario = r.Comentario!.Trim(),
        IdClasificacion = _catalogos.ObtenerId(Catalogo.Clasificacion, r.Clasificacion!),
        PuntajeSatisfaccion = Conversiones.Byte(r.PuntajeSatisfaccion!)
    };

    protected override string Id(Encuesta e) => e.IdOpinion.ToString();
    protected override string ClaveContenido(Encuesta e) =>
        ClaveOpinion.Crear(e.IdCliente, e.IdProducto, e.Fecha, e.Comentario);

    protected override bool Insertar(Encuesta encuesta, ResultadoCarga resultado)
    {
        _provisionales.AsegurarCliente(encuesta.IdCliente, resultado);
        _provisionales.AsegurarProducto(encuesta.IdProducto, resultado);
        return _dao.Insertar(encuesta);
    }
}
