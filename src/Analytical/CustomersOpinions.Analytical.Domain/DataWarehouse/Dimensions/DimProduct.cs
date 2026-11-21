namespace CustomersOpinions.Analytical.Domain.DataWarehouse.Dimensions;

public sealed class DimProduct
{
    public int ProductKey { get; set; }

    public int SourceProductId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public bool IsInferred { get; set; }
}
