using System.Text;
using CustomersOpinions.Data.Services;
using CustomersOpinions.Load;

// uso del proceso: 
//   dotnet run --project CustomersOpinions.Load            -> carga los CSV originales
//   dotnet run --project CustomersOpinions.Load -- sucios  -> prueba de validaciones (no guarda nada)

Console.OutputEncoding = Encoding.UTF8;

try
{
    var config = ConfiguracionCarga.Leer(args);
    ResumenConsola.ImprimirEncabezado(config);

    var resumen = CargaFactory.Ejecutar(config.ConnectionString, config.CarpetaCsv, config.Simulacion, config.CrearProvisionales);
    ResumenConsola.Imprimir(resumen);

    if (resumen.Rechazados > 0)
        Console.WriteLine($"Detalle de rechazados guardado en: {RechazosWriter.Guardar(resumen.Rechazos, config.CarpetaRechazados)}");

    return resumen.Archivos.Any(a => a.ErrorArchivo != null) ? 2 : 0;
}
catch (Exception ex)
{
    ResumenConsola.ImprimirError($"No se pudo ejecutar la carga: {ex.Message}");
    return 1;
}
