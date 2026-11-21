using System.Globalization;
using CustomersOpinions.Analytical.Domain.Common;
using CustomersOpinions.Analytical.Domain.Staging;
using CustomersOpinions.Analytical.Infrastructure.Configuration;
using CustomersOpinions.Analytical.Infrastructure.Extraction.Base;
using CustomersOpinions.Analytical.Infrastructure.Persistence;
using CustomersOpinions.Analytical.Infrastructure.Sources.Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomersOpinions.Analytical.Infrastructure.Extraction.Api;

/// <summary>API component: social comments → <c>Staging.SocialComments</c>. A null customer is a valid anonymous comment.</summary>
public sealed class SocialCommentApiExtractor : ApiExtractorBase<SocialCommentDto, StagingSocialComment>
{
    private readonly SocialCommentsApiClient _client;
    private readonly string _endpointUrl;

    public SocialCommentApiExtractor(SocialCommentsApiClient client, IOptions<SocialCommentsSourceOptions> options,
        TimeProvider timeProvider, ILogger<SocialCommentApiExtractor> logger)
        : base(options.Value.MaxPages, StagingTableMaps.SocialComments, timeProvider, logger)
    {
        _client = client;
        _endpointUrl = options.Value.EndpointUrl;
    }

    /// <summary>Endpoint URL (never the API key), written to <c>SourceEndpoint</c>.</summary>
    public override string Name => _endpointUrl;

    protected override Task<Result<PagedResponse<SocialCommentDto>>> GetPageAsync(int pageNumber, CancellationToken cancellationToken) =>
        _client.GetPageAsync(pageNumber, cancellationToken);

    protected override string? KeyOf(SocialCommentDto dto) => dto.CommentId;

    protected override StagingSocialComment Map(SocialCommentDto dto) => new()
    {
        CommentId = dto.CommentId,
        CustomerId = dto.CustomerId?.ToString(CultureInfo.InvariantCulture),
        ProductId = dto.ProductId?.ToString(CultureInfo.InvariantCulture),
        Platform = dto.Platform,
        CommentDate = dto.CommentDate,
        Comment = dto.Comment
    };
}
