using CustomersOpinions.Data.Daos;
using CustomersOpinions.Data.Services.Cargadores;
using CustomersOpinions.Data.Services.Estado;
using CustomersOpinions.Data.Services.Lectura;

namespace CustomersOpinions.Data.Services;

public static class CargaFactory
{
    public static ResumenCarga Ejecutar(string connectionString, string carpetaCsv, bool simulacion, bool crearProvisionales)
    {
        using var session = new DbSession(connectionString, simulacion);

        var lector = new CsvReaderService();
        var existentes = new ExistentesDao(session);
        var clienteDao = new ClienteDao(session);
        var productoDao = new ProductoDao(session);
        var catalogos = new CatalogoCache(new CatalogoDao(session));
        var referencias = new ReferenciasCarga(clienteDao, productoDao);
        var provisionales = new Provisionales(clienteDao, productoDao, catalogos, referencias, crearProvisionales);

        var cargadores = new ICargadorArchivo[]
        {
            new ClientesCargador(lector, existentes, clienteDao, referencias),
            new ProductosCargador(lector, existentes, productoDao, catalogos, referencias),
            new FuentesDatosCargador(lector, existentes, new FuenteDatosDao(session), catalogos),
            new EncuestasCargador(lector, existentes, new EncuestaDao(session), catalogos, provisionales),
            new ComentariosSocialesCargador(lector, existentes, new ComentarioSocialDao(session), catalogos, referencias),
            new ResenasWebCargador(lector, existentes, new ResenaWebDao(session), referencias)
        };

        var estados = new IEstadoCarga[] { catalogos, referencias };
        return new LoadService(session, estados, cargadores).Ejecutar(carpetaCsv);
    }
}
