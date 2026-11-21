namespace CustomersOpinions.Analytical.Domain.DataWarehouse.Dimensions;

public sealed class DimClassification
{
    public int ClassificationKey { get; set; }

    public string Name { get; set; } = string.Empty;

    public short? SentimentValue { get; set; }
}
