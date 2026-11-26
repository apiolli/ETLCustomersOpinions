using System.Data;
using CustomersOpinions.Analytical.Domain.DataWarehouse.Dimensions;
using CustomersOpinions.Analytical.Infrastructure.Configuration;
using CustomersOpinions.Analytical.Infrastructure.Loading.Base;
using CustomersOpinions.Analytical.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomersOpinions.Analytical.Infrastructure.Loading;

public sealed class EventDimensionLoader : DimensionLoaderBase
{
    public EventDimensionLoader(IDbConnectionFactory connections, IOptions<DimensionLoadOptions> options,
        ILogger<EventDimensionLoader> logger) : base(connections, options, logger)
    {
    }

    public override string DimensionName => "Event";

    public override string TableName => "dbo.DimEvento";

    public override int Order => 60;

    protected override async Task<LoadCounts> LoadCoreAsync(SqlConnection connection, SqlTransaction transaction,
        CancellationToken cancellationToken)
    {
        var existing = (await QueryAsync(connection, transaction, DimensionQueries.ExistingEvents,
            r => new DimEvent
            {
                Name = r.GetString(0),
                EventType = r.GetString(1),
                StartDate = r.IsDBNull(2) ? null : DateOnly.FromDateTime(r.GetDateTime(2)),
                EndDate = r.IsDBNull(3) ? null : DateOnly.FromDateTime(r.GetDateTime(3))
            }, cancellationToken)).ToDictionary(e => e.Name, StringComparer.OrdinalIgnoreCase);

        await using var command = Procedure(connection, transaction, DimensionQueries.LoadEvent);
        var name = command.Parameters.Add("@Nombre", SqlDbType.VarChar, 100);
        var type = command.Parameters.Add("@TipoEvento", SqlDbType.VarChar, 30);
        var start = command.Parameters.Add("@FechaInicio", SqlDbType.Date);
        var end = command.Parameters.Add("@FechaFin", SqlDbType.Date);

        int inserted = 0, updated = 0, unchanged = 0, rejected = 0;
        foreach (var options in Options.Events)
        {
            if (string.IsNullOrWhiteSpace(options.Name) || string.IsNullOrWhiteSpace(options.Type)
                || (options.StartDate.HasValue && options.EndDate.HasValue && options.EndDate < options.StartDate))
            {
                rejected++;
                Logger.LogWarning("Event rejected: {Name} needs a name, a type and a valid date range", options.Name);
                continue;
            }

            var item = new DimEvent
            {
                Name = Clip(options.Name, 100, string.Empty),
                EventType = Clip(options.Type, 30, string.Empty),
                StartDate = options.StartDate,
                EndDate = options.EndDate
            };

            if (existing.TryGetValue(item.Name, out var current))
            {
                if (current.EventType == item.EventType && current.StartDate == item.StartDate && current.EndDate == item.EndDate)
                {
                    unchanged++;
                    continue;
                }
                updated++;
            }
            else
            {
                inserted++;
            }

            name.Value = item.Name;
            type.Value = item.EventType;
            start.Value = item.StartDate?.ToDateTime(TimeOnly.MinValue) ?? (object)DBNull.Value;
            end.Value = item.EndDate?.ToDateTime(TimeOnly.MinValue) ?? (object)DBNull.Value;
            await command.ExecuteNonQueryAsync(cancellationToken);
            existing[item.Name] = item;
        }

        return new LoadCounts(Options.Events.Count, inserted, updated, unchanged, rejected);
    }
}
