namespace CustomersOpinions.Analytical.Application.Abstractions.Extraction;

/// <summary>Kind of data source a extractor reads.</summary>
public enum SourceKind
{
    File,
    Database,
    Api
}

/// <summary>Non generic part of every extractor.</summary>
public interface IExtractor
{
    /// <summary>Name of the origin (file, view or endpoint), written to the staging metadata column.</summary>
    string Name { get; }

    /// <summary>Kind of source.</summary>
    SourceKind SourceKind { get; }
}
