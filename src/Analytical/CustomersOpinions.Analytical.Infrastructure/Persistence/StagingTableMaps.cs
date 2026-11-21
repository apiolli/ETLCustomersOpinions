using CustomersOpinions.Analytical.Domain.Staging;

namespace CustomersOpinions.Analytical.Infrastructure.Persistence;

/// <summary>
/// Maps of the five staging tables, matching <c>database/analytical/02_CreateStaging.sql</c>.
/// Table names come only from here, never from configuration or input.
/// </summary>
public static class StagingTableMaps
{
    private const string Schema = "Staging";

    public static readonly StagingTableMap<StagingSurvey> Surveys = new(Schema, "Surveys",
        new[]
        {
            Text("SurveyId", 50), Text("CustomerId", 50), Text("ProductId", 50), Text("SurveyDate", 50),
            Text("Comment", null), Text("Classification", 50), Text("SatisfactionScore", 20), Text("Source", 100),
            Text("SourceFile", 260), Timestamp("ExtractedAt")
        },
        r => new object?[]
        {
            r.SurveyId, r.CustomerId, r.ProductId, r.SurveyDate, r.Comment, r.Classification,
            r.SatisfactionScore, r.Source, r.SourceName, r.ExtractedAt
        });

    public static readonly StagingTableMap<StagingWebReview> WebReviews = new(Schema, "WebReviews",
        new[]
        {
            Text("ReviewId", 50), Text("CustomerId", 50), Text("ProductId", 50), Text("ReviewDate", 50),
            Text("Comment", null), Text("Rating", 20), Text("SourceObject", 128), Timestamp("ExtractedAt")
        },
        r => new object?[]
        {
            r.ReviewId, r.CustomerId, r.ProductId, r.ReviewDate, r.Comment, r.Rating, r.SourceName, r.ExtractedAt
        });

    public static readonly StagingTableMap<StagingSocialComment> SocialComments = new(Schema, "SocialComments",
        new[]
        {
            Text("CommentId", 50), Text("CustomerId", 50), Text("ProductId", 50), Text("Platform", 50),
            Text("CommentDate", 50), Text("Comment", null), Text("SourceEndpoint", 500), Timestamp("ExtractedAt")
        },
        r => new object?[]
        {
            r.CommentId, r.CustomerId, r.ProductId, r.Platform, r.CommentDate, r.Comment, r.SourceName, r.ExtractedAt
        });

    public static readonly StagingTableMap<StagingCustomer> Customers = new(Schema, "Customers",
        new[]
        {
            Text("CustomerId", 50), Text("Name", 200), Text("Email", 200), Text("IsInferred", 5),
            Text("SourceObject", 128), Timestamp("ExtractedAt")
        },
        r => new object?[] { r.CustomerId, r.Name, r.Email, r.IsInferred, r.SourceName, r.ExtractedAt });

    public static readonly StagingTableMap<StagingProduct> Products = new(Schema, "Products",
        new[]
        {
            Text("ProductId", 50), Text("Name", 200), Text("Category", 100), Text("IsInferred", 5),
            Text("SourceObject", 128), Timestamp("ExtractedAt")
        },
        r => new object?[] { r.ProductId, r.Name, r.Category, r.IsInferred, r.SourceName, r.ExtractedAt });

    /// <summary>Qualified names of every known staging table (allow list for TRUNCATE).</summary>
    public static readonly IReadOnlySet<string> KnownTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Surveys.QualifiedName, WebReviews.QualifiedName, SocialComments.QualifiedName,
        Customers.QualifiedName, Products.QualifiedName
    };

    private static StagingColumn Text(string name, int? maxLength) => new(name, typeof(string), maxLength);

    private static StagingColumn Timestamp(string name) => new(name, typeof(DateTime), null);
}
