namespace CustomersOpinions.Analytical.Domain.Staging;

/// <summary>Product master data for <c>DimProducto</c> (table <c>Staging.Products</c>).</summary>
public sealed record StagingProduct : StagingRecord
{
    public string? ProductId { get; init; }
    public string? Name { get; init; }
    public string? Category { get; init; }

    /// <summary>"1" or "0".</summary>
    public string? IsInferred { get; init; }
}
