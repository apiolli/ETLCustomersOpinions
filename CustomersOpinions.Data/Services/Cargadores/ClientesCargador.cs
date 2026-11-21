using CustomersOpinions.Data.Csvs;
using CustomersOpinions.Data.Daos.Interfaces;
using CustomersOpinions.Data.Models;
using CustomersOpinions.Data.Services.Estado;
using CustomersOpinions.Data.Services.Lectura;
using CustomersOpinions.Data.Services.Validacion;
using CustomersOpinions.Data.Services.Exceptions;

namespace CustomersOpinions.Data.Services.Cargadores;

public class ClientesCargador : CargadorArchivo<ClienteCsv, Cliente>
{
    private readonly IInsertarDao<Cliente> _dao;
    private readonly IReferencias _referencias;
    public ClientesCargador(ILectorCsv lector, IExistentesDao existentes, IInsertarDao<Cliente> dao, IReferencias referencias)
        : base(lector, existentes)
    {
        _dao = dao;
        _referencias = referencias;
    }

    public override string Archivo => "clients.csv";
    protected override Tabla Tabla => Tabla.Cliente;
    protected override string CampoId => "IdCliente";
    protected override string? IdOriginal(ClienteCsv r) => r.IdCliente;
    protected override void Validar(ClienteCsv r, Validador v)
    {
        v.EnteroPositivo(r.IdCliente, "IdCliente");
        v.Requerido(r.Nombre, "Nombre", 100);
        v.EmailOpcional(r.Email, 150);
    }

    protected override Cliente Transformar(ClienteCsv r) => new()
    {
        IdCliente = Conversiones.Entero(r.IdCliente!),
        Nombre = r.Nombre!.Trim(),
        Email = Conversiones.Nulo(r.Email)?.ToLowerInvariant()
    };

    protected override string Id(Cliente c) => c.IdCliente.ToString();

    protected override bool Insertar(Cliente cliente, ResultadoCarga resultado)
    {
        if (cliente.Email != null && _referencias.EmailRegistrado(cliente.Email))
            throw new ValidationException($"Email duplicado '{cliente.Email}'");

        if (!_dao.Insertar(cliente))
            return false;

        _referencias.RegistrarCliente(cliente.IdCliente, cliente.Email);
        return true;
    }
}
