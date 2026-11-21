using CustomersOpinions.Data.Daos.Interfaces;
using CustomersOpinions.Data.Models;
using CustomersOpinions.Data.Services.Exceptions;
using CustomersOpinions.Data.Services.Lectura;
using CustomersOpinions.Data.Services.Validacion;
using Microsoft.Data.SqlClient;

namespace CustomersOpinions.Data.Services.Cargadores;

public abstract class CargadorArchivo<TCsv, TModelo> : ICargadorArchivo
{
    private static readonly HashSet<int> ErroresDeRegistro = new() { 547, 2601, 2627, 2628, 8152 };
    private readonly ILectorCsv _lector;
    private readonly IExistentesDao _existentes;

    protected CargadorArchivo(ILectorCsv lector, IExistentesDao existentes)
    {
        _lector = lector;
        _existentes = existentes;
    }

    public abstract string Archivo { get; }
    protected abstract Tabla Tabla { get; }
    protected abstract string CampoId { get; }
    protected abstract string? IdOriginal(TCsv registro);
    protected abstract void Validar(TCsv registro, Validador validador);
    protected abstract TModelo Transformar(TCsv registro);
    protected abstract string Id(TModelo entidad);
    protected virtual string? ClaveContenido(TModelo entidad) => null;
    protected abstract bool Insertar(TModelo entidad, ResultadoCarga resultado);

    public void Procesar(string ruta, ResultadoCarga resultado)
    {
        var duplicados = new Duplicados(
            new HashSet<string>(_existentes.ObtenerIds(Tabla), StringComparer.OrdinalIgnoreCase),
            () => _existentes.ObtenerClavesContenido(Tabla));

        foreach (var fila in _lector.Leer<TCsv>(ruta))
        {
            resultado.Procesados++;
            if (fila.Error != null)
            {
                resultado.Rechazar(fila.Numero, null, fila.Error);
                continue;
            }

            try
            {
                ProcesarRegistro(fila.Registro!, duplicados, resultado);
            }
            catch (RegistroRechazadoException ex)
            {
                resultado.Rechazar(fila.Numero, IdOriginal(fila.Registro!), ex.Message);
            }
            catch (SqlException ex) when (ErroresDeRegistro.Contains(ex.Number))
            {
                resultado.Rechazar(fila.Numero, IdOriginal(fila.Registro!), $"Rechazado por la base de datos: {ex.Message}");
            }
        }
    }

    private void ProcesarRegistro(TCsv registro, Duplicados duplicados, ResultadoCarga resultado)
    {
        var validador = new Validador();
        Validar(registro, validador);
        validador.LanzarSiHayErrores();

        var entidad = Transformar(registro);
        var id = Id(entidad);
        var clave = ClaveContenido(entidad);
        duplicados.Revisar(CampoId, id, clave);

        if (!Insertar(entidad, resultado))
            throw new ValidationException($"{CampoId} {id} ya existe en la base");

        duplicados.RegistrarInsertado(clave);
        resultado.Insertados++;
    }

    private sealed class Duplicados
    {
        private readonly HashSet<string> _idsEnArchivo = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _idsEnBase;
        private readonly Func<IReadOnlyList<string>> _leerClavesEnBase;
        private HashSet<string>? _claves;  

        public Duplicados(HashSet<string> idsEnBase, Func<IReadOnlyList<string>> leerClavesEnBase)
        {
            _idsEnBase = idsEnBase;
            _leerClavesEnBase = leerClavesEnBase;
        }

        public void Revisar(string campoId, string id, string? clave)
        {
            if (!_idsEnArchivo.Add(id))
                throw new ValidationException($"{campoId} {id} repetido en el archivo");

            if (_idsEnBase.Contains(id))
                throw new ValidationException($"{campoId} {id} ya existe en la base");

            if (clave != null && Claves().Contains(clave))
                throw new ValidationException("Opinión duplicada: mismo cliente, producto, fecha y comentario que otro registro");
        }

        public void RegistrarInsertado(string? clave)
        {
            if (clave != null)
                Claves().Add(clave);
        }

        private HashSet<string> Claves() =>
            _claves ??= new HashSet<string>(_leerClavesEnBase(), StringComparer.OrdinalIgnoreCase);
    }
}
