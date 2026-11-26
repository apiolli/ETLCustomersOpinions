using System.Data;
using CustomersOpinions.Analytical.Infrastructure.Configuration;
using CustomersOpinions.Analytical.Infrastructure.Loading.Base;
using CustomersOpinions.Analytical.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomersOpinions.Analytical.Infrastructure.Loading;

public sealed class TimeDimensionLoader : DimensionLoaderBase
{
    public TimeDimensionLoader(IDbConnectionFactory connections, IOptions<DimensionLoadOptions> options,
        ILogger<TimeDimensionLoader> logger) : base(connections, options, logger)
    {
    }

    public override string DimensionName => "Time";

    public override string TableName => "dbo.DimTiempo";

    public override int Order => 20;

    protected override async Task<LoadCounts> LoadCoreAsync(SqlConnection connection, SqlTransaction transaction,
        CancellationToken cancellationToken)
    {
        var range = (await QueryAsync(connection, transaction, DimensionQueries.StagingDateRange,
            r => (Min: r.IsDBNull(0) ? (DateTime?)null : r.GetDateTime(0),
                  Max: r.IsDBNull(1) ? (DateTime?)null : r.GetDateTime(1),
                  Invalid: r.IsDBNull(2) ? 0 : r.GetInt32(2)),
            cancellationToken)).Single();

        var dates = Options.Events
            .SelectMany(e => new[] { e.StartDate, e.EndDate })
            .Where(d => d.HasValue)
            .Select(d => d!.Value)
            .ToList();
        if (range.Min.HasValue)
            dates.Add(DateOnly.FromDateTime(range.Min.Value));
        if (range.Max.HasValue)
            dates.Add(DateOnly.FromDateTime(range.Max.Value));

        if (dates.Count == 0)
            return new LoadCounts(0, 0, 0, 0, range.Invalid, "No dates found in staging");

        var start = new DateOnly(dates.Min().Year, 1, 1);
        var end = new DateOnly(dates.Max().Year, 12, 31);
        var days = end.DayNumber - start.DayNumber + 1;

        await using var command = Procedure(connection, transaction, DimensionQueries.LoadTime);
        command.Parameters.Add("@FechaInicio", SqlDbType.Date).Value = start.ToDateTime(TimeOnly.MinValue);
        command.Parameters.Add("@FechaFin", SqlDbType.Date).Value = end.ToDateTime(TimeOnly.MinValue);
        var insertedParameter = command.Parameters.Add("@Insertados", SqlDbType.Int);
        insertedParameter.Direction = ParameterDirection.Output;
        await command.ExecuteNonQueryAsync(cancellationToken);

        var inserted = (int)insertedParameter.Value;
        if (range.Invalid > 0)
            Logger.LogWarning("{Count} staging dates could not be parsed as yyyy-MM-dd", range.Invalid);

        return new LoadCounts(days, inserted, 0, days - inserted, range.Invalid,
            $"Range {start:yyyy-MM-dd} to {end:yyyy-MM-dd}");
    }
}
