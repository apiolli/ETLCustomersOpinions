namespace CustomersOpinions.Transactional.Data.Services.State;

/// <summary>In-memory state that must be reloaded from the database after a file is rolled back.</summary>
public interface ILoadState
{
    /// <summary>Discards the cached state and reads it again.</summary>
    void Reload();
}
