using System.Data;
using CustomersOpinions.Analytical.Application.Loading;
using CustomersOpinions.Analytical.Domain.DataWarehouse.Dimensions;
using CustomersOpinions.Analytical.Infrastructure.Configuration;
using CustomersOpinions.Analytical.Infrastructure.Loading.Base;
using CustomersOpinions.Analytical.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomersOpinions.Analytical.Infrastructure.Loading;

public sealed class KeywordDimensionLoader : DimensionLoaderBase
{
    public KeywordDimensionLoader(IDbConnectionFactory connections, IOptions<DimensionLoadOptions> options,
        ILogger<KeywordDimensionLoader> logger) : base(connections, options, logger)
    {
    }

    public override string DimensionName => "Keyword";

    public override string TableName => "dbo.DimPalabraClave";

    public override int Order => 70;

    protected override async Task<LoadCounts> LoadCoreAsync(SqlConnection connection, SqlTransaction transaction,
        CancellationToken cancellationToken)
    {
        var comments = await QueryAsync(connection, transaction, DimensionQueries.StagingComments,
            r => Text(r, 0), cancellationToken);

        var keywords = comments
            .SelectMany(c => KeywordExtractor.Extract(c, Options.KeywordMinLength))
            .GroupBy(w => w, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() >= Options.KeywordMinOccurrences)
            .Select(g => new DimKeyword { Word = g.Key })
            .OrderBy(k => k.Word, StringComparer.Ordinal)
            .ToList();

        var existing = (await QueryAsync(connection, transaction, DimensionQueries.ExistingKeywords,
            r => r.GetString(0), cancellationToken)).ToHashSet(StringComparer.OrdinalIgnoreCase);

        await using var command = Procedure(connection, transaction, DimensionQueries.LoadKeyword);
        var word = command.Parameters.Add("@Palabra", SqlDbType.VarChar, KeywordExtractor.MaxWordLength);
        var key = command.Parameters.Add("@PalabraKey", SqlDbType.Int);
        key.Direction = ParameterDirection.Output;

        var inserted = 0;
        foreach (var keyword in keywords)
        {
            word.Value = keyword.Word;
            await command.ExecuteNonQueryAsync(cancellationToken);
            keyword.KeywordKey = (int)key.Value;
            if (existing.Add(keyword.Word))
                inserted++;
        }

        return new LoadCounts(keywords.Count, inserted, 0, keywords.Count - inserted, 0,
            $"{comments.Count:N0} comments scanned");
    }
}
