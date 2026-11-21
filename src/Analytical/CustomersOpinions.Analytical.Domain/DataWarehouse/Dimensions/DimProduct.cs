namespace CustomersOpinions.Analytical.Domain.DataWarehouse.Dimensions;

/// <summary>Table <c>DimProducto</c>.</summary>
public sealed class DimProduct
{
    /// <summary>Column <c>ProductoKey</c>.</summary>
    public int ProductKey { get; set; }

    /// <summary>Column <c>IdProductoOrigen</c>.</summary>
    public int SourceProductId { get; set; }

    /// <summary>Column <c>Nombre</c>.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Column <c>Categoria</c>.</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Column <c>EsInferido</c>.</summary>
    public bool IsInferred { get; set; }
}
