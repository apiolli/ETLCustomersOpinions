namespace CustomersOpinions.Data.Services.Lectura;

public interface ILectorCsv
{
    IEnumerable<FilaCsv<T>> Leer<T>(string ruta);
}
