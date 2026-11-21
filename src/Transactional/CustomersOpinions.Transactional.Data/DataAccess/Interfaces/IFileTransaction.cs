namespace CustomersOpinions.Transactional.Data.DataAccess.Interfaces;

/// <summary>One transaction (or savepoint, in dry-run mode) per loaded file.</summary>
public interface IFileTransaction
{
    /// <summary>True when nothing is persisted (dirty files validation run).</summary>
    bool IsDryRun { get; }

    /// <summary>Starts the unit of work of one file.</summary>
    void BeginFile(string name);

    /// <summary>Commits the current file.</summary>
    void CommitFile();

    /// <summary>Rolls back the current file.</summary>
    void RollbackFile();
}
