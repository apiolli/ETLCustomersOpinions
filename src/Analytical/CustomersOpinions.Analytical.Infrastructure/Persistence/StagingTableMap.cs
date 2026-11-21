using CustomersOpinions.Analytical.Domain.Common;

namespace CustomersOpinions.Analytical.Infrastructure.Persistence;

public sealed record StagingColumn(string Name, Type Type, int? MaxLength);

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

    public string QualifiedName => $"{Schema}.{Table}";

    public IReadOnlyList<StagingColumn> Columns { get; }

    public Func<T, object?[]> Values { get; }

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
