namespace CustomersOpinions.Data.Services;

public class ResultadoCarga
{
    public ResultadoCarga(string archivo) => Archivo = archivo;
    public string Archivo { get; }
    public int Procesados { get; set; }
    public int Insertados { get; set; }
    public int ClientesProvisionales { get; set; }
    public int ProductosProvisionales { get; set; }
    public List<Rechazo> Rechazos { get; } = new();
    public int Rechazados => Rechazos.Count;
    public string? ErrorArchivo { get; set; }

    public void Rechazar(int fila, string? idRegistro, string motivo) =>
        Rechazos.Add(new Rechazo(Archivo, fila, idRegistro, motivo));

    public void Revertir(string error)
    {
        ErrorArchivo = error;
        Insertados = 0;
        ClientesProvisionales = 0;
        ProductosProvisionales = 0;
    }
}

