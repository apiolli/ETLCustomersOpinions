namespace CustomersOpinions.Analytical.Domain.DataWarehouse.Dimensions;

public sealed class DimTime
{
    public int TimeKey { get; set; }

    public DateOnly Date { get; set; }

    public short Year { get; set; }

    public byte Semester { get; set; }

    public byte Quarter { get; set; }

    public byte Month { get; set; }

    public string MonthName { get; set; } = string.Empty;

    public string YearMonth { get; set; } = string.Empty;

    public byte WeekOfYear { get; set; }

    public byte Day { get; set; }

    public byte DayOfWeek { get; set; }

    public string DayName { get; set; } = string.Empty;

    public bool IsWeekend { get; set; }
}
