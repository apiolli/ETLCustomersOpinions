using System.Data;
using System.Diagnostics;
using CustomersOpinions.Analytical.Application.Abstractions.Loading;
using CustomersOpinions.Analytical.Application.Reporting;
using CustomersOpinions.Analytical.Infrastructure.Configuration;
using CustomersOpinions.Analytical.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomersOpinions.Analytical.Infrastructure.Loading.Base;

public abstract class DimensionLoaderBase : IDimensionLoader
{
    private readonly IDbConnectionFactory _connections;

    protected DimensionLoaderBase(IDbConnectionFactory connections, IOptions<DimensionLoadOptions> options, ILogger logger)
    {
        _connections = connections;
        Options = options.Value;
        Logger = logger;
    }

    public abstract string DimensionName { get; }

    public abstract string TableName { get; }

    public abstract int Order { get; }

    protected DimensionLoadOptions Options { get; }

    protected ILogger Logger { get; }

    public async Task<DimensionLoadResult> LoadAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await using var connection = (SqlConnection)_connections.Create(ConnectionNames.AnalyticalDb);
            await connection.OpenAsync(cancellationToken);
            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

            var counts = await LoadCoreAsync(connection, transaction, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            stopwatch.Stop();

            var status = counts.Rejected > 0 ? SourceStatus.CompletedWithRejections : SourceStatus.Succeeded;
            return new DimensionLoadResult(DimensionName, TableName, status, counts.Read, counts.Inserted,
                counts.Updated, counts.Unchanged, counts.Rejected, stopwatch.Elapsed, counts.Message);
        }
        catch (SqlException ex)
        {
            return DimensionLoadResult.Failed(DimensionName, TableName, stopwatch.Elapsed,
                $"SQL error {ex.Number}: {ex.Message}");
        }
    }

    protected abstract Task<LoadCounts> LoadCoreAsync(SqlConnection connection, SqlTransaction transaction,
        CancellationToken cancellationToken);

    protected SqlCommand Procedure(SqlConnection connection, SqlTransaction transaction, string name)
    {
        var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = name;
        command.CommandTimeout = Options.CommandTimeoutSeconds;
        return command;
    }

    protected async Task<List<T>> QueryAsync<T>(SqlConnection connection, SqlTransaction transaction, string sql,
        Func<SqlDataReader, T> map, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        command.CommandTimeout = Options.CommandTimeoutSeconds;

        var rows = new List<T>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            rows.Add(map(reader));
        return rows;
    }

    protected async Task<int> CountAsync(SqlConnection connection, SqlTransaction transaction, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = $"select count(*) from {TableName};";
        command.CommandTimeout = Options.CommandTimeoutSeconds;
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
    }

    protected static string? Text(SqlDataReader reader, int ordinal) =>
        reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);

    protected static string Clip(string? value, int maxLength, string fallback)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return fallback;
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }

    protected static bool Flag(string? value) =>
        value?.Trim() is "1" or "true" or "True" or "TRUE";
}
