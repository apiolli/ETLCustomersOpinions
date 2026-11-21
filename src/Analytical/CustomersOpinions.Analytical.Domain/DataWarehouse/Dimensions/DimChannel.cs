namespace CustomersOpinions.Analytical.Domain.DataWarehouse.Dimensions;

public sealed class DimChannel
{
    public int ChannelKey { get; set; }

    public string Channel { get; set; } = string.Empty;

    public string Platform { get; set; } = string.Empty;

    public string SourceType { get; set; } = string.Empty;
}
