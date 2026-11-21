using System.Data;
using CustomersOpinions.Transactional.Data.DataAccess.Interfaces;
using Microsoft.Data.SqlClient;

namespace CustomersOpinions.Transactional.Data.DataAccess;

public sealed class DbSession : IFileTransaction, IDisposable
{
    private readonly SqlConnection _connection;
    private SqlTransaction? _transaction;
    private string? _savepoint;

    public DbSession(string connectionString, bool dryRun)
    {
        IsDryRun = dryRun;
        _connection = new SqlConnection(connectionString);
        _connection.Open();
        if (IsDryRun)
            _transaction = _connection.BeginTransaction();
    }

    public bool IsDryRun { get; }

    public void BeginFile(string name)
    {
        if (IsDryRun)
        {
            _savepoint = name;
            _transaction!.Save(name);
        }
        else
        {
            _transaction = _connection.BeginTransaction();
        }
    }

    public void CommitFile()
    {
        if (IsDryRun || _transaction == null) return;
        _transaction.Commit();
        CloseTransaction();
    }

    public void RollbackFile()
    {
        if (_transaction == null) return;

        try
        {
            if (IsDryRun)
                _transaction.Rollback(_savepoint!);
            else
                _transaction.Rollback();
        }
        catch (InvalidOperationException)
        {
        }

        if (!IsDryRun)
            CloseTransaction();
    }

    public SqlCommand CreateCommand(string procedure) =>
        new(procedure, _connection, _transaction) { CommandType = CommandType.StoredProcedure };

    public void Dispose()
    {
        if (IsDryRun && _transaction != null)
        {
            try { _transaction.Rollback(); } catch (InvalidOperationException) { }
        }
        _transaction?.Dispose();
        _connection.Dispose();
    }

    private void CloseTransaction()
    {
        _transaction!.Dispose();
        _transaction = null;
    }
}
