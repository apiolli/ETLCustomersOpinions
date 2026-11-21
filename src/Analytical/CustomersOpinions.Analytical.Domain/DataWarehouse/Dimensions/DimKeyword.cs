namespace CustomersOpinions.Analytical.Domain.DataWarehouse.Dimensions;

public sealed class DimKeyword
{
    public int KeywordKey { get; set; }

    public string Word { get; set; } = string.Empty;
}
