namespace CustomersOpinions.Analytical.Domain.DataWarehouse.Dimensions;

public sealed class DimCustomer
{
    public int CustomerKey { get; set; }

    public int? SourceCustomerId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string Country { get; set; } = string.Empty;

    public string AgeRange { get; set; } = string.Empty;

    public string CustomerType { get; set; } = string.Empty;

    public bool IsInferred { get; set; }
}
