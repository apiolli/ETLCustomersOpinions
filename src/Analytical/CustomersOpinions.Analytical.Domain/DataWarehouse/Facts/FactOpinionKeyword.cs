namespace CustomersOpinions.Analytical.Domain.DataWarehouse.Facts;

public sealed class FactOpinionKeyword
{
    public long OpinionKey { get; set; }

    public int KeywordKey { get; set; }

    public byte Frequency { get; set; }
}
