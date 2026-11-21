namespace CustomersOpinions.Analytical.Domain.DataWarehouse.Facts;

/// <summary>Table <c>FactOpinion</c>.</summary>
public sealed class FactOpinion
{
    /// <summary>Column <c>OpinionKey</c>.</summary>
    public long OpinionKey { get; set; }

    /// <summary>Column <c>TiempoKey</c>.</summary>
    public int TimeKey { get; set; }

    /// <summary>Column <c>ProductoKey</c>.</summary>
    public int ProductKey { get; set; }

    /// <summary>Column <c>ClienteKey</c>.</summary>
    public int CustomerKey { get; set; }

    /// <summary>Column <c>CanalKey</c>.</summary>
    public int ChannelKey { get; set; }

    /// <summary>Column <c>ClasificacionKey</c>.</summary>
    public int ClassificationKey { get; set; }

    /// <summary>Column <c>EventoKey</c>.</summary>
    public int EventKey { get; set; }

    /// <summary>Column <c>IdOpinionOrigen</c>.</summary>
    public string SourceOpinionId { get; set; } = string.Empty;

    /// <summary>Column <c>Comentario</c>.</summary>
    public string Comment { get; set; } = string.Empty;

    /// <summary>Column <c>Cantidad</c>.</summary>
    public byte Quantity { get; set; }

    /// <summary>Column <c>Puntaje</c>; null for social comments.</summary>
    public byte? Score { get; set; }

    /// <summary>Computed column <c>EsSatisfecho</c> (read only).</summary>
    public bool? IsSatisfied { get; init; }

    /// <summary>Column <c>FechaCarga</c>.</summary>
    public DateTime LoadedAt { get; set; }
}
