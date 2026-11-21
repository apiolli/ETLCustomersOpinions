namespace CustomersOpinions.Analytical.Domain.DataWarehouse.Dimensions;

/// <summary>Table <c>DimCanal</c>.</summary>
public sealed class DimChannel
{
    /// <summary>Column <c>CanalKey</c>.</summary>
    public int ChannelKey { get; set; }

    /// <summary>Column <c>Canal</c>.</summary>
    public string Channel { get; set; } = string.Empty;

    /// <summary>Column <c>Plataforma</c>.</summary>
    public string Platform { get; set; } = string.Empty;

    /// <summary>Column <c>TipoOrigen</c>.</summary>
    public string SourceType { get; set; } = string.Empty;
}
