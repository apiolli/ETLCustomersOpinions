namespace CustomersOpinions.Analytical.Domain.Staging;

public sealed record StagingCustomer : StagingRecord
{
    public string? CustomerId { get; init; }
    public string? Name { get; init; }
    public string? Email { get; init; }

    public string? IsInferred { get; init; }
}
