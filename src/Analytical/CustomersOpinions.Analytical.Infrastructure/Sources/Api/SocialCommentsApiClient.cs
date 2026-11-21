using System.Globalization;
using System.Net.Http.Json;
using CustomersOpinions.Analytical.Domain.Common;
using CustomersOpinions.Analytical.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace CustomersOpinions.Analytical.Infrastructure.Sources.Api;

public sealed class SocialCommentsApiClient
{
    private readonly HttpClient _http;
    private readonly SocialCommentsSourceOptions _options;

    public SocialCommentsApiClient(HttpClient http, IOptions<SocialCommentsSourceOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    public async Task<Result<PagedResponse<SocialCommentDto>>> GetPageAsync(int pageNumber, CancellationToken cancellationToken)
    {
        var uri = string.Create(CultureInfo.InvariantCulture,
            $"{_options.Endpoint}?pageNumber={pageNumber}&pageSize={_options.PageSize}");

        using var response = await _http.GetAsync(uri, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return Result<PagedResponse<SocialCommentDto>>.Failure(ErrorType.SourceUnavailable,
                $"API returned {(int)response.StatusCode} {response.ReasonPhrase} for page {pageNumber}");

        var page = await response.Content.ReadFromJsonAsync<PagedResponse<SocialCommentDto>>(cancellationToken);
        return page is null
            ? Result<PagedResponse<SocialCommentDto>>.Failure(ErrorType.SourceUnavailable, $"Empty API response for page {pageNumber}")
            : Result<PagedResponse<SocialCommentDto>>.Success(page);
    }
}
