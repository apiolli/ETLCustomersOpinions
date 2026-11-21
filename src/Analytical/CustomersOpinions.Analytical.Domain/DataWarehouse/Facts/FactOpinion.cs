namespace CustomersOpinions.Analytical.Domain.DataWarehouse.Facts;

public sealed class FactOpinion
{
    public long OpinionKey { get; set; }

    public int TimeKey { get; set; }

    public int ProductKey { get; set; }

    public int CustomerKey { get; set; }

    public int ChannelKey { get; set; }

    public int ClassificationKey { get; set; }

    public int EventKey { get; set; }

    public string SourceOpinionId { get; set; } = string.Empty;

    public string Comment { get; set; } = string.Empty;

    public byte Quantity { get; set; }

    public byte? Score { get; set; }

    public bool? IsSatisfied { get; init; }

    public DateTime LoadedAt { get; set; }
}
