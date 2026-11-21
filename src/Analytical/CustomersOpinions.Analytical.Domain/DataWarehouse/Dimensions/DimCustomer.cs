namespace CustomersOpinions.Analytical.Domain.DataWarehouse.Dimensions;

/// <summary>Table <c>DimCliente</c>.</summary>
public sealed class DimCustomer
{
    /// <summary>Column <c>ClienteKey</c>.</summary>
    public int CustomerKey { get; set; }

    /// <summary>Column <c>IdClienteOrigen</c>; null for the anonymous member.</summary>
    public int? SourceCustomerId { get; set; }

    /// <summary>Column <c>Nombre</c>.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Column <c>Email</c>.</summary>
    public string? Email { get; set; }

    /// <summary>Column <c>Pais</c>.</summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>Column <c>RangoEdad</c>.</summary>
    public string AgeRange { get; set; } = string.Empty;

    /// <summary>Column <c>TipoCliente</c>.</summary>
    public string CustomerType { get; set; } = string.Empty;

    /// <summary>Column <c>EsInferido</c>.</summary>
    public bool IsInferred { get; set; }
}
