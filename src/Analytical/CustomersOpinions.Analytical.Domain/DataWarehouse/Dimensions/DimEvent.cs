namespace CustomersOpinions.Analytical.Domain.DataWarehouse.Dimensions;

/// <summary>Table <c>DimEvento</c>.</summary>
public sealed class DimEvent
{
    /// <summary>Column <c>EventoKey</c>.</summary>
    public int EventKey { get; set; }

    /// <summary>Column <c>Nombre</c>.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Column <c>TipoEvento</c>.</summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>Column <c>FechaInicio</c>.</summary>
    public DateOnly? StartDate { get; set; }

    /// <summary>Column <c>FechaFin</c>.</summary>
    public DateOnly? EndDate { get; set; }
}
