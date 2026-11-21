using CustomersOpinions.Data.Services;

namespace CustomersOpinions.Load;

internal static class ResumenConsola
{
    private const int MaximoRechazosEnPantalla = 60;
    private static readonly string Separador = new('-', 58);

    public static void ImprimirEncabezado(ConfiguracionCarga config)
    {
        Console.WriteLine("CustomersOpinions - Carga de OpinionesClientesDB");
        Console.WriteLine($"Origen:             {config.CarpetaCsv}");
        Console.WriteLine($"Modo:               {(config.Simulacion ? "SIMULACIÓN con CSV sucios (se revierte al final)" : "Carga normal")}");
        Console.WriteLine($"CrearProvisionales: {config.CrearProvisionales}");
        Console.WriteLine();
    }

    public static void Imprimir(ResumenCarga resumen)
    {
        Console.WriteLine($"{"Archivo",-22}{"Procesados",12}{(resumen.Simulacion ? "Válidos" : "Insertados"),12}{"Rechazados",12}");
        Console.WriteLine(Separador);
        foreach (var a in resumen.Archivos)
        {
            Console.WriteLine($"{a.Archivo,-22}{a.Procesados,12}{a.Insertados,12}{a.Rechazados,12}");
            if (a.ErrorArchivo != null)
                ImprimirError($"  ARCHIVO REVERTIDO: {a.ErrorArchivo}");
        }
        Console.WriteLine(Separador);
        Console.WriteLine($"{"TOTAL",-22}{resumen.Procesados,12}{resumen.Insertados,12}{resumen.Rechazados,12}");
        Console.WriteLine();
        Console.WriteLine($"Clientes provisionales creados:  {resumen.ClientesProvisionales}");
        Console.WriteLine($"Productos provisionales creados: {resumen.ProductosProvisionales}");
        Console.WriteLine();

        ImprimirRechazos(resumen);

        if (resumen.Simulacion)
            Console.WriteLine("Simulación terminada: se hizo rollback, la base de datos no cambió.");
    }

    public static void ImprimirError(string mensaje)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(mensaje);
        Console.ResetColor();
    }

    private static void ImprimirRechazos(ResumenCarga resumen)
    {
        if (resumen.Rechazados == 0) return;

        Console.WriteLine("Motivos de rechazo:");
        foreach (var r in resumen.Rechazos.Take(MaximoRechazosEnPantalla))
            Console.WriteLine($"  [{r.Archivo} fila {r.Fila}] {r.IdRegistro ?? "-"}: {r.Motivo}");
        if (resumen.Rechazados > MaximoRechazosEnPantalla)
            Console.WriteLine($"  ... y {resumen.Rechazados - MaximoRechazosEnPantalla} más (ver el CSV de rechazados)");
        Console.WriteLine();
    }
}
