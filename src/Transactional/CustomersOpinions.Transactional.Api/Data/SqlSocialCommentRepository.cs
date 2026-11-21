using System.Data;
using System.Globalization;
using CustomersOpinions.Transactional.Api.Contracts;
using Microsoft.Data.SqlClient;

namespace CustomersOpinions.Transactional.Api.Data;

/// <summary>ADO.NET implementation over <c>dbo.usp_GetSocialCommentsPage</c>.</summary>
public sealed class SqlSocialCommentRepository : ISocialCommentRepository
{
    private const string PageProcedure = "dbo.usp_GetSocialCommentsPage";
    private readonly string _connectionString;
    private readonly ILogger<SqlSocialCommentRepository> _logger;

    public SqlSocialCommentRepository(IConfiguration configuration, ILogger<SqlSocialCommentRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("TransactionalDb")
            ?? throw new InvalidOperationException("Missing connection string 'TransactionalDb'.");
        _logger = logger;
    }

    public async Task<(IReadOnlyList<SocialCommentResponse> Items, int TotalCount)> GetPageAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(PageProcedure, connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.Add("@PageNumber", SqlDbType.Int).Value = pageNumber;
        command.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;
        var total = command.Parameters.Add("@TotalCount", SqlDbType.Int);
        total.Direction = ParameterDirection.Output;

        var items = new List<SocialCommentResponse>();
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                items.Add(new SocialCommentResponse(
                    CommentId: reader.GetString(0),
                    CustomerId: await reader.IsDBNullAsync(1, cancellationToken) ? null : reader.GetInt32(1),
                    ProductId: reader.GetInt32(2),
                    Platform: reader.GetString(3),
                    CommentDate: reader.GetDateTime(4).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Comment: reader.GetString(5)));
            }
        }

        // the output parameter is available only after the reader is closed
        var totalCount = total.Value is int value ? value : 0;
        _logger.LogInformation("Social comments page {PageNumber} (size {PageSize}) returned {Count} of {Total}",
            pageNumber, pageSize, items.Count, totalCount);
        return (items, totalCount);
    }

    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = new SqlCommand("select 1", connection) { CommandTimeout = 5 };
            await command.ExecuteScalarAsync(cancellationToken);
            return true;
        }
        catch (SqlException ex)
        {
            _logger.LogWarning("Health check failed: {Message}", ex.Message);
            return false;
        }
    }
}
