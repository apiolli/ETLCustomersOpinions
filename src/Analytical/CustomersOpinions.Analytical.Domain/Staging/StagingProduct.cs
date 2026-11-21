namespace CustomersOpinions.Analytical.Domain.Staging;

public sealed record StagingProduct : StagingRecord
{
    public string? ProductId { get; init; }
    public string? Name { get; init; }
    public string? Category { get; init; }

    public string? IsInferred { get; init; }
}
