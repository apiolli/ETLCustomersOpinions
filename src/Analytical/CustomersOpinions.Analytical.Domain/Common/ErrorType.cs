namespace CustomersOpinions.Analytical.Domain.Common;

/// <summary>Category of an expected failure carried by a <see cref="Result"/>.</summary>
public enum ErrorType
{
    /// <summary>No error (successful result).</summary>
    None,

    /// <summary>A single record is invalid; the source continues.</summary>
    Validation,

    /// <summary>The source cannot be read (missing file, database or API down, bad structure).</summary>
    SourceUnavailable,

    /// <summary>Writing to the staging tables failed.</summary>
    Persistence,

    /// <summary>An unexpected error.</summary>
    Unexpected
}
