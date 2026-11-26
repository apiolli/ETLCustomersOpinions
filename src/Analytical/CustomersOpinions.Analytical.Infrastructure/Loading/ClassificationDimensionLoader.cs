using CustomersOpinions.Analytical.Infrastructure.Configuration;
using CustomersOpinions.Analytical.Infrastructure.Loading.Base;
using CustomersOpinions.Analytical.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomersOpinions.Analytical.Infrastructure.Loading;

public sealed class ClassificationDimensionLoader : DimensionLoaderBase
{
    public ClassificationDimensionLoader(IDbConnectionFactory connections, IOptions<DimensionLoadOptions> options,
        ILogger<ClassificationDimensionLoader> logger) : base(connections, options, logger)
    {
    }

    public override string DimensionName => "Classification";

    public override string TableName => "dbo.DimClasificacion";

    public override int Order => 10;

    protected override async Task<LoadCounts> LoadCoreAsync(SqlConnection connection, SqlTransaction transaction,
        CancellationToken cancellationToken)
    {
        var before = await CountAsync(connection, transaction, cancellationToken);
        await using (var seed = Procedure(connection, transaction, DimensionQueries.SeedMembers))
            await seed.ExecuteNonQueryAsync(cancellationToken);
        var inserted = await CountAsync(connection, transaction, cancellationToken) - before;

        var known = (await QueryAsync(connection, transaction, DimensionQueries.ExistingClassifications,
            r => r.GetString(0), cancellationToken)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var labels = await QueryAsync(connection, transaction, DimensionQueries.StagingClassifications,
            r => r.GetString(0), cancellationToken);

        var unknown = labels.Where(l => !known.Contains(l)).ToList();
        foreach (var label in unknown)
            Logger.LogWarning("Classification {Label} from staging has no member in {Table}; facts will use 'Sin clasificar'",
                label, TableName);

        var message = unknown.Count == 0
            ? "Default members ensured"
            : $"Unknown labels: {string.Join(", ", unknown)}";
        return new LoadCounts(labels.Count, inserted, 0, Math.Max(0, labels.Count - unknown.Count - inserted),
            unknown.Count, message);
    }
}
