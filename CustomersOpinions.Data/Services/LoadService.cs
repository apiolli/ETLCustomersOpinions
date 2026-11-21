using CustomersOpinions.Data.Daos.Interfaces;
using CustomersOpinions.Data.Services.Cargadores;
using CustomersOpinions.Data.Services.Estado;

namespace CustomersOpinions.Data.Services;
public class LoadService
{
    private readonly ITransaccionArchivo _transaccion;
    private readonly IReadOnlyList<IEstadoCarga> _estados;
    private readonly IReadOnlyList<ICargadorArchivo> _cargadores;

    public LoadService(ITransaccionArchivo transaccion, IReadOnlyList<IEstadoCarga> estados,
        IReadOnlyList<ICargadorArchivo> cargadores)
    {
        _transaccion = transaccion;
        _estados = estados;
        _cargadores = cargadores;
    }

    public ResumenCarga Ejecutar(string carpetaCsv)
    {
        var resumen = new ResumenCarga { Simulacion = _transaccion.EsSimulacion };
        RecargarEstado();

        foreach (var cargador in _cargadores)
        {
            resumen.Archivos.Add(Procesar(cargador, carpetaCsv));
        }

        return resumen;
    }

    private ResultadoCarga Procesar(ICargadorArchivo cargador, string carpetaCsv)
    {
        var resultado = new ResultadoCarga(cargador.Archivo);
        try
        {
            _transaccion.IniciarArchivo(Path.GetFileNameWithoutExtension(cargador.Archivo));
            cargador.Procesar(Path.Combine(carpetaCsv, cargador.Archivo), resultado);
            _transaccion.ConfirmarArchivo();
        }
        catch (Exception ex)
        {
            _transaccion.RevertirArchivo();
            resultado.Revertir(ex.Message);
            RecargarEstado();
        }
        return resultado;
    }

    private void RecargarEstado()
    {
        foreach (var estado in _estados)
        {
            estado.Recargar();
        }
    }
}
