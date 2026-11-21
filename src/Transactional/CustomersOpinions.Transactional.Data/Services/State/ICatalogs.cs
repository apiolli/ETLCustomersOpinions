using CustomersOpinions.Transactional.Data.Models;

namespace CustomersOpinions.Transactional.Data.Services.State;

public interface ICatalogs : ILoadState
{
    int GetId(Catalog catalog, string name);
}
