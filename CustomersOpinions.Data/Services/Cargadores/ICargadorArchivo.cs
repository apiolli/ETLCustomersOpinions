namespace CustomersOpinions.Data.Services.Cargadores;

public interface ICargadorArchivo
{
    string Archivo { get; }
    void Procesar(string ruta, ResultadoCarga resultado);
}
