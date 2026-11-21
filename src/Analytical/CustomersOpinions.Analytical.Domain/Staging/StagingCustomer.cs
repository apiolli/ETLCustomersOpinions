namespace CustomersOpinions.Analytical.Domain.Staging;

/// <summary>Customer master data for <c>DimCliente</c> (table <c>Staging.Customers</c>).</summary>
public sealed record StagingCustomer : StagingRecord
{
    public string? CustomerId { get; init; }
    public string? Name { get; init; }
    public string? Email { get; init; }

    /// <summary>"1" or "0".</summary>
    public string? IsInferred { get; init; }
}
