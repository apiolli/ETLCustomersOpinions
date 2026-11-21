namespace CustomersOpinions.Analytical.Domain.DataWarehouse.Dimensions;

public sealed class DimEvent
{
    public int EventKey { get; set; }

    public string Name { get; set; } = string.Empty;

    public string EventType { get; set; } = string.Empty;

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }
}
