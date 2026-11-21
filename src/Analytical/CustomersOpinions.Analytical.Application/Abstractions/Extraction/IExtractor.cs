namespace CustomersOpinions.Analytical.Application.Abstractions.Extraction;

public enum SourceKind
{
    File,
    Database,
    Api
}

public interface IExtractor
{
    string Name { get; }

    SourceKind SourceKind { get; }
}
