using System.Data;
using CustomersOpinions.Analytical.Domain.DataWarehouse.Dimensions;
using CustomersOpinions.Analytical.Infrastructure.Configuration;
using CustomersOpinions.Analytical.Infrastructure.Loading.Base;
using CustomersOpinions.Analytical.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomersOpinions.Analytical.Infrastructure.Loading;

public sealed class ChannelDimensionLoader : DimensionLoaderBase
{
    public ChannelDimensionLoader(IDbConnectionFactory connections, IOptions<DimensionLoadOptions> options,
        ILogger<ChannelDimensionLoader> logger) : base(connections, options, logger)
    {
    }

    public override string DimensionName => "Channel";

    public override string TableName => "dbo.DimCanal";

    public override int Order => 50;

    protected override async Task<LoadCounts> LoadCoreAsync(SqlConnection connection, SqlTransaction transaction,
        CancellationToken cancellationToken)
    {
        var channels = await QueryAsync(connection, transaction, DimensionQueries.StagingChannels,
            r => ToChannel(r.GetString(0), Text(r, 1)), cancellationToken);

        var before = await CountAsync(connection, transaction, cancellationToken);

        await using var command = Procedure(connection, transaction, DimensionQueries.LoadChannel);
        var channel = command.Parameters.Add("@Canal", SqlDbType.VarChar, 30);
        var platform = command.Parameters.Add("@Plataforma", SqlDbType.VarChar, 30);
        var sourceType = command.Parameters.Add("@TipoOrigen", SqlDbType.VarChar, 30);
        foreach (var item in channels)
        {
            channel.Value = item.Channel;
            platform.Value = item.Platform;
            sourceType.Value = item.SourceType;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        var inserted = await CountAsync(connection, transaction, cancellationToken) - before;
        return new LoadCounts(channels.Count, inserted, 0, channels.Count - inserted);
    }

    private static DimChannel ToChannel(string kind, string? platform) => kind switch
    {
        "Survey" => new DimChannel { Channel = "Encuesta interna", Platform = "Formulario interno", SourceType = "Archivo CSV" },
        "WebReview" => new DimChannel { Channel = "Reseña web", Platform = "Sitio web", SourceType = "BD relacional" },
        _ => new DimChannel { Channel = "Red social", Platform = Clip(platform, 30, "Desconocida"), SourceType = "API REST" }
    };
}
