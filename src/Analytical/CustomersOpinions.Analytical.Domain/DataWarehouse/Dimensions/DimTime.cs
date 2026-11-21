namespace CustomersOpinions.Analytical.Domain.DataWarehouse.Dimensions;

/// <summary>Table <c>DimTiempo</c>.</summary>
public sealed class DimTime
{
    /// <summary>Column <c>TiempoKey</c> (yyyyMMdd).</summary>
    public int TimeKey { get; set; }

    /// <summary>Column <c>Fecha</c>.</summary>
    public DateOnly Date { get; set; }

    /// <summary>Column <c>Anio</c>.</summary>
    public short Year { get; set; }

    /// <summary>Column <c>Semestre</c>.</summary>
    public byte Semester { get; set; }

    /// <summary>Column <c>Trimestre</c>.</summary>
    public byte Quarter { get; set; }

    /// <summary>Column <c>Mes</c>.</summary>
    public byte Month { get; set; }

    /// <summary>Column <c>NombreMes</c>.</summary>
    public string MonthName { get; set; } = string.Empty;

    /// <summary>Column <c>AnioMes</c> (yyyy-MM).</summary>
    public string YearMonth { get; set; } = string.Empty;

    /// <summary>Column <c>SemanaAnio</c>.</summary>
    public byte WeekOfYear { get; set; }

    /// <summary>Column <c>Dia</c>.</summary>
    public byte Day { get; set; }

    /// <summary>Column <c>DiaSemana</c>.</summary>
    public byte DayOfWeek { get; set; }

    /// <summary>Column <c>NombreDia</c>.</summary>
    public string DayName { get; set; } = string.Empty;

    /// <summary>Column <c>EsFinDeSemana</c>.</summary>
    public bool IsWeekend { get; set; }
}
