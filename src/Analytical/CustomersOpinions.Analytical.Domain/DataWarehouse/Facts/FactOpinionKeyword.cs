namespace CustomersOpinions.Analytical.Domain.DataWarehouse.Facts;

/// <summary>Table <c>FactOpinionPalabra</c>.</summary>
public sealed class FactOpinionKeyword
{
    /// <summary>Column <c>OpinionKey</c>.</summary>
    public long OpinionKey { get; set; }

    /// <summary>Column <c>PalabraKey</c>.</summary>
    public int KeywordKey { get; set; }

    /// <summary>Column <c>Frecuencia</c>.</summary>
    public byte Frequency { get; set; }
}
