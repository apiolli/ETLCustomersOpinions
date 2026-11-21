using System.ComponentModel.DataAnnotations;

namespace CustomersOpinions.Analytical.Infrastructure.Configuration;

/// <summary>
/// Sections <c>Sources:WebReviews</c>, <c>Sources:Customers</c> and <c>Sources:Products</c>
/// (bound as named options, one name per section).
/// </summary>
public sealed class DatabaseSourceOptions
{
    public const string WebReviews = "WebReviews";
    public const string Customers = "Customers";
    public const string Products = "Products";

    public bool Enabled { get; set; } = true;

    [Range(1, 3600)]
    public int CommandTimeoutSeconds { get; set; } = 120;
}
