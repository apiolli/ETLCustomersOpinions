using CustomersOpinions.Transactional.Data.DataAccess.Interfaces;
using CustomersOpinions.Transactional.Data.Services.Loaders;
using CustomersOpinions.Transactional.Data.Services.State;

namespace CustomersOpinions.Transactional.Data.Services;

public sealed class LoadService
{
    private readonly IFileTransaction _transaction;
    private readonly IReadOnlyList<ILoadState> _states;
    private readonly IReadOnlyList<IFileLoader> _loaders;

    public LoadService(IFileTransaction transaction, IReadOnlyList<ILoadState> states,
        IReadOnlyList<IFileLoader> loaders)
    {
        _transaction = transaction;
        _states = states;
        _loaders = loaders;
    }

    public LoadSummary Run(string csvFolder)
    {
        var summary = new LoadSummary { DryRun = _transaction.IsDryRun };
        ReloadState();

        foreach (var loader in _loaders)
        {
            summary.Files.Add(Process(loader, csvFolder));
        }

        return summary;
    }

    private LoadResult Process(IFileLoader loader, string csvFolder)
    {
        var result = new LoadResult(loader.FileName);
        try
        {
            _transaction.BeginFile(Path.GetFileNameWithoutExtension(loader.FileName));
            loader.Process(Path.Combine(csvFolder, loader.FileName), result);
            _transaction.CommitFile();
        }
        catch (Exception ex)
        {
            _transaction.RollbackFile();
            result.RollBack(ex.Message);
            ReloadState();
        }
        return result;
    }

    private void ReloadState()
    {
        foreach (var state in _states)
        {
            state.Reload();
        }
    }
}
