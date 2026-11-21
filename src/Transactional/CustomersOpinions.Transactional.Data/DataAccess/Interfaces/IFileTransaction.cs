namespace CustomersOpinions.Transactional.Data.DataAccess.Interfaces;

public interface IFileTransaction
{
    bool IsDryRun { get; }

    void BeginFile(string name);

    void CommitFile();

    void RollbackFile();
}
