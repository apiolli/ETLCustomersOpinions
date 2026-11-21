using CustomersOpinions.Analytical.Application.Abstractions.Reading;
using CustomersOpinions.Analytical.Domain.Common;
using Microsoft.Extensions.DependencyInjection;

namespace CustomersOpinions.Analytical.Infrastructure.Reading;

public sealed class FileReaderResolver : IFileReaderResolver
{
    private readonly IServiceProvider _services;

    public FileReaderResolver(IServiceProvider services) => _services = services;

    public Result<IFileReader<TRow>> Resolve<TRow>(string path)
    {
        var extension = Path.GetExtension(path);
        var reader = _services.GetServices<IFileReader<TRow>>()
            .FirstOrDefault(r => r.SupportedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase));

        return reader is null
            ? Result<IFileReader<TRow>>.Failure(ErrorType.SourceUnavailable, $"Unsupported file type '{extension}' for {Path.GetFileName(path)}")
            : Result<IFileReader<TRow>>.Success(reader);
    }
}
