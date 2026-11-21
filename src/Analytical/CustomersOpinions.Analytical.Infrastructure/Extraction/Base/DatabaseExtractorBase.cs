using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using CustomersOpinions.Analytical.Application.Abstractions.Extraction;
using CustomersOpinions.Analytical.Domain.Common;
using CustomersOpinions.Analytical.Domain.Staging;
using CustomersOpinions.Analytical.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace CustomersOpinions.Analytical.Infrastructure.Extraction.Base;

/// <summary>
/// Extractor over a relational query: opens a connection to <c>TransactionalDb</c>, runs
/// <see cref="Query"/> with <see cref="CommandBehavior.SequentialAccess"/> and streams the rows.
/// </summary>
/// <typeparam name="TRecord">Staging record type.</typeparam>
public abstract class DatabaseExtractorBase<TRecord> : ExtractorBase<TRecord> where TRecord : StagingRecord
{
    private readonly IDbConnectionFactory _connections;
    private readonly int _commandTimeoutSeconds;

    protected DatabaseExtractorBase(IDbConnectionFactory connections, int commandTimeoutSeconds,
        StagingTableMap<TRecord> map, TimeProvider timeProvider, ILogger logger) : base(map, timeProvider, logger)
    {
        _connections = connections;
        _commandTimeoutSeconds = commandTimeoutSeconds;
    }

    public override SourceKind SourceKind => SourceKind.Database;

    /// <summary>SELECT with an explicit column list, ordered by the source id.</summary>
    protected abstract string Query { get; }

    /// <summary>Maps the current row (columns must be read in order).</summary>
    protected abstract TRecord Map(DbDataReader reader);

    protected override async IAsyncEnumerable<Result<TRecord>> ExtractCoreAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using var connection = _connections.Create(ConnectionNames.TransactionalDb);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = Query;
        command.CommandType = CommandType.Text;
        command.CommandTimeout = _commandTimeoutSeconds;

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            yield return Result<TRecord>.Success(Map(reader));
    }
}
