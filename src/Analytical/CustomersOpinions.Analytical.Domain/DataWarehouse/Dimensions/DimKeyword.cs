namespace CustomersOpinions.Analytical.Domain.DataWarehouse.Dimensions;

/// <summary>Table <c>DimPalabraClave</c>.</summary>
public sealed class DimKeyword
{
    /// <summary>Column <c>PalabraKey</c>.</summary>
    public int KeywordKey { get; set; }

    /// <summary>Column <c>Palabra</c>.</summary>
    public string Word { get; set; } = string.Empty;
}
