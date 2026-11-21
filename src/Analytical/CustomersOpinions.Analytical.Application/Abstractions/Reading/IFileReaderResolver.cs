using CustomersOpinions.Analytical.Domain.Common;

namespace CustomersOpinions.Analytical.Application.Abstractions.Reading;

public interface IFileReaderResolver
{
    Result<IFileReader<TRow>> Resolve<TRow>(string path);
}
