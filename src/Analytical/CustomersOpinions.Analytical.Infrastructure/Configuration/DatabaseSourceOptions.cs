using System.ComponentModel.DataAnnotations;

namespace CustomersOpinions.Analytical.Infrastructure.Configuration;

public sealed class DatabaseSourceOptions
{
    public const string WebReviews = "WebReviews";
    public const string Customers = "Customers";
    public const string Products = "Products";

    public bool Enabled { get; set; } = true;

    [Range(1, 3600)]
    public int CommandTimeoutSeconds { get; set; } = 120;
}
