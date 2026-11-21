namespace CustomersOpinions.Analytical.Domain.DataWarehouse.Dimensions;

/// <summary>Table <c>DimClasificacion</c>.</summary>
public sealed class DimClassification
{
    /// <summary>Column <c>ClasificacionKey</c>.</summary>
    public int ClassificationKey { get; set; }

    /// <summary>Column <c>Nombre</c>.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Column <c>ValorSentimiento</c>.</summary>
    public short? SentimentValue { get; set; }
}
