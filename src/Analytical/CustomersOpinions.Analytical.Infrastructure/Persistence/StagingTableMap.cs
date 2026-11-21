using CustomersOpinions.Analytical.Domain.Common;

namespace CustomersOpinions.Analytical.Infrastructure.Persistence;

/// <summary>One column of a staging table.</summary>
/// <param name="Name">Column name.</param>
/// <param name="Type">CLR type of the values (<see cref="string"/> or <see cref="DateTime"/>).</param>
/// <param name="MaxLength">varchar length; null for varchar(max) and non text columns.</param>
public sealed record StagingColumn(string Name, Type Type, int? MaxLength);

/// <summary>How a staging record maps to its table: schema, table, ordered columns and values.</summary>
/// <typeparam name="T">Staging record type.</typeparam>
public sealed class StagingTableMap<T>
{
    public StagingTableMap(string schema, string table, IReadOnlyList<StagingColumn> columns, Func<T, object?[]> values)
    {
        Schema = schema;
        Table = table;
        Columns = columns;
        Values = values;
    }

    public string Schema { get; }

    public string Table { get; }

    /// <summary><c>Schema.Table</c>.</summary>
    public string QualifiedName => $"{Schema}.{Table}";

    public IReadOnlyList<StagingColumn> Columns { get; }

    /// <summary>Values of a record, in the order of <see cref="Columns"/>.</summary>
    public Func<T, object?[]> Values { get; }

    /// <summary>
    /// Rejects a record whose text value is longer than its column, so a single long value
    /// never makes a whole bulk insert batch fail.
    /// </summary>
    public Result CheckLengths(T record)
    {
        var values = Values(record);
        for (var i = 0; i < Columns.Count; i++)
        {
            if (Columns[i].MaxLength is { } max && values[i] is string text && text.Length > max)
                return Result.Failure(ErrorType.Validation,
                    $"{Columns[i].Name} exceeds {max} characters ({text.Length})");
        }
        return Result.Success();
    }
}
