namespace CustomersOpinions.Data.Daos.Interfaces;

public interface ITransaccionArchivo
{
    bool EsSimulacion { get; }
    void IniciarArchivo(string nombre);
    void ConfirmarArchivo();
    void RevertirArchivo();
}
