using CustomersOpinions.Transactional.Api.Configuration;
using CustomersOpinions.Transactional.Api.Contracts;
using CustomersOpinions.Transactional.Api.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace CustomersOpinions.Transactional.Api.Endpoints;

/// <summary>Maps <c>GET /api/v1/social-comments</c> and <c>GET /health</c>.</summary>
public static class SocialCommentEndpoints
{
    public static IEndpointRouteBuilder MapSocialCommentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/social-comments", GetPageAsync)
            .WithName("GetSocialComments")
            .Produces<PagedResponse<SocialCommentResponse>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);

        app.MapGet("/health", HealthAsync).WithName("Health");
        return app;
    }

    private static async Task<IResult> GetPageAsync(
        ISocialCommentRepository repository,
        IOptions<PagingOptions> paging,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken,
        int pageNumber = 1,
        int? pageSize = null)
    {
        var size = pageSize ?? paging.Value.DefaultPageSize;
        if (pageNumber < 1 || size < 1)
            return Results.Problem("pageNumber and pageSize must be greater than or equal to 1.",
                statusCode: StatusCodes.Status400BadRequest);

        size = Math.Min(size, paging.Value.MaxPageSize);

        try
        {
            var (items, totalCount) = await repository.GetPageAsync(pageNumber, size, cancellationToken);
            return Results.Ok(PagedResponse<SocialCommentResponse>.Create(pageNumber, size, totalCount, items));
        }
        catch (SqlException ex)
        {
            loggerFactory.CreateLogger(nameof(SocialCommentEndpoints))
                .LogError("Database unavailable while reading social comments: {Message}", ex.Message);
            return Results.Problem("The database is not available.", statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }

    private static async Task<IResult> HealthAsync(ISocialCommentRepository repository, CancellationToken cancellationToken) =>
        await repository.IsHealthyAsync(cancellationToken)
            ? Results.Ok(new { status = "Healthy" })
            : Results.Json(new { status = "Unhealthy" }, statusCode: StatusCodes.Status503ServiceUnavailable);
}
