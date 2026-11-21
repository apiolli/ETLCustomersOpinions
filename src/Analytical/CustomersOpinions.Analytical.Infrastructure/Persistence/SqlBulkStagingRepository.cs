using System.Data;
using CustomersOpinions.Analytical.Application.Abstractions.Persistence;
using CustomersOpinions.Analytical.Domain.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace CustomersOpinions.Analytical.Infrastructure.Persistence;

/// <summary>Generic staging repository: <c>SqlBulkCopy</c> per batch, configured by a <see cref="StagingTableMap{T}"/>.</summary>
/// <typeparam name="T">Staging record type.</typeparam>
public sealed class SqlBulkStagingRepository<T> : IStagingRepository<T>
{
    private const int BulkCopyTimeoutSeconds = 300;

    private readonly IDbConnectionFactory _connections;
    private readonly StagingTableMap<T> _map;
    private readonly ILogger<SqlBulkStagingRepository<T>> _logger;

    public SqlBulkStagingRepository(IDbConnectionFactory connections, StagingTableMap<T> map,
        ILogger<SqlBulkStagingRepository<T>> logger)
    {
        if (!StagingTableMaps.KnownTables.Contains(map.QualifiedName))
            throw new ArgumentException($"Unknown staging table '{map.QualifiedName}'.", nameof(map));

        _connections = connections;
        _map = map;
        _logger = logger;
    }

    public string TableName => _map.QualifiedName;

    public async Task<Result> TruncateAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = (SqlConnection)_connections.Create(ConnectionNames.AnalyticalDb);
            await connection.OpenAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            // the name comes from the allow listed map, never from input
            command.CommandText = $"truncate table [{_map.Schema}].[{_map.Table}];";
            await command.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogDebug("Truncated {Table}", TableName);
            return Result.Success();
        }
        catch (SqlException ex)
        {
            return Result.Failure(ErrorType.Persistence, $"Cannot truncate {TableName}: {ex.Message}");
        }
    }

    public async Task<Result<int>> BulkInsertAsync(IReadOnlyCollection<T> batch, CancellationToken cancellationToken)
    {
        if (batch.Count == 0)
            return Result<int>.Success(0);

        try
        {
            using var table = ToDataTable(batch);
            await using var connection = (SqlConnection)_connections.Create(ConnectionNames.AnalyticalDb);
            await connection.OpenAsync(cancellationToken);
            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

            using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.TableLock, transaction)
            {
                DestinationTableName = $"[{_map.Schema}].[{_map.Table}]",
                BulkCopyTimeout = BulkCopyTimeoutSeconds,
                BatchSize = batch.Count
            };
            foreach (var column in _map.Columns)
                bulkCopy.ColumnMappings.Add(column.Name, column.Name);

            await bulkCopy.WriteToServerAsync(table, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result<int>.Success(batch.Count);
        }
        catch (SqlException ex)
        {
            return Result<int>.Failure(ErrorType.Persistence, $"Bulk insert into {TableName} failed: {ex.Message}");
        }
    }

    private DataTable ToDataTable(IReadOnlyCollection<T> batch)
    {
        var table = new DataTable();
        foreach (var column in _map.Columns)
            table.Columns.Add(column.Name, column.Type);

        foreach (var record in batch)
        {
            var values = _map.Values(record);
            for (var i = 0; i < values.Length; i++)
                values[i] ??= DBNull.Value;
            table.Rows.Add(values);
        }
        return table;
    }
}
