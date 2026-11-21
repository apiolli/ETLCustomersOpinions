using System.Data.Common;
using CustomersOpinions.Analytical.Domain.Staging;
using CustomersOpinions.Analytical.Infrastructure.Configuration;
using CustomersOpinions.Analytical.Infrastructure.Extraction.Base;
using CustomersOpinions.Analytical.Infrastructure.Persistence;
using CustomersOpinions.Analytical.Infrastructure.Sources.Database;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomersOpinions.Analytical.Infrastructure.Extraction.Database;

public sealed class WebReviewDatabaseExtractor : DatabaseExtractorBase<StagingWebReview>
{
    public WebReviewDatabaseExtractor(IDbConnectionFactory connections, IOptionsMonitor<DatabaseSourceOptions> options,
        TimeProvider timeProvider, ILogger<WebReviewDatabaseExtractor> logger)
        : base(connections, options.Get(DatabaseSourceOptions.WebReviews).CommandTimeoutSeconds,
            StagingTableMaps.WebReviews, timeProvider, logger)
    {
    }

    public override string Name => ExtractionQueries.WebReviewsView;

    protected override string Query => ExtractionQueries.WebReviews;

    protected override StagingWebReview Map(DbDataReader reader)
    {
        var row = WebReviewRow.Read(reader);
        return new StagingWebReview
        {
            ReviewId = row.IdReview,
            CustomerId = row.IdCliente,
            ProductId = row.IdProducto,
            ReviewDate = row.Fecha,
            Comment = row.Comentario,
            Rating = row.Rating
        };
    }
}
