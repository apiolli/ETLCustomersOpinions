using System.Data;
using CustomersOpinions.Data.Daos.Interfaces;
using Microsoft.Data.SqlClient;

namespace CustomersOpinions.Data.Daos;

public sealed class DbSession : ITransaccionArchivo, IDisposable
{
    private readonly SqlConnection _conexion;
    private SqlTransaction? _transaccion;
    private string? _savepoint;

    public DbSession(string connectionString, bool simulacion)
    {
        EsSimulacion = simulacion;
        _conexion = new SqlConnection(connectionString);
        _conexion.Open();
        if (EsSimulacion)
            _transaccion = _conexion.BeginTransaction();
    }

    public bool EsSimulacion { get; }

    public void IniciarArchivo(string nombre)
    {
        if (EsSimulacion)
        {
            _savepoint = nombre;
            _transaccion!.Save(nombre);
        }
        else
        {
            _transaccion = _conexion.BeginTransaction();
        }
    }

    public void ConfirmarArchivo()
    {
        if (EsSimulacion || _transaccion == null) return;
        _transaccion.Commit();
        CerrarTransaccion();
    }

    public void RevertirArchivo()
    {
        if (_transaccion == null) return;

        try
        {
            if (EsSimulacion)
                _transaccion.Rollback(_savepoint!);
            else
                _transaccion.Rollback();
        }
        catch (InvalidOperationException)
        {
            // aqui la transaccion ya fue terminada por el servidor, no hay nada que revertir
        }

        if (!EsSimulacion)
            CerrarTransaccion();
    }

    public SqlCommand CrearComando(string procedimiento) =>
        new(procedimiento, _conexion, _transaccion) { CommandType = CommandType.StoredProcedure };

    public void Dispose()
    {
        if (EsSimulacion && _transaccion != null)
        {
            try { _transaccion.Rollback(); } catch (InvalidOperationException) { }
        }
        _transaccion?.Dispose();
        _conexion.Dispose();
    }

    private void CerrarTransaccion()
    {
        _transaccion!.Dispose();
        _transaccion = null;
    }
}
