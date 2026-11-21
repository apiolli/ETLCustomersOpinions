using System.ComponentModel.DataAnnotations;

namespace CustomersOpinions.Analytical.Infrastructure.Configuration;

/// <summary>Section <c>Sources:SocialComments</c>.</summary>
public sealed class SocialCommentsSourceOptions : IValidatableObject
{
    public const string SectionName = "Sources:SocialComments";

    public bool Enabled { get; set; } = true;

    public string BaseUrl { get; set; } = "https://localhost:7181";

    [Required(AllowEmptyStrings = false)]
    public string Endpoint { get; set; } = "/api/v1/social-comments";

    [Range(1, 1000)]
    public int PageSize { get; set; } = 500;

    /// <summary>Safety limit of pages read in one run.</summary>
    [Range(1, 1_000_000)]
    public int MaxPages { get; set; } = 10_000;

    [Range(1, 3600)]
    public int TimeoutSeconds { get; set; } = 30;

    [Range(1, 10)]
    public int RetryCount { get; set; } = 3;

    /// <summary>Secret: value sent in the <c>X-Api-Key</c> header.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Endpoint URL without credentials, written to <c>Staging.SocialComments.SourceEndpoint</c>.</summary>
    public string EndpointUrl => $"{BaseUrl.TrimEnd('/')}/{Endpoint.TrimStart('/')}";

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Enabled)
            yield break;

        if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            yield return new ValidationResult("Sources:SocialComments:BaseUrl must be an absolute HTTPS URL.", new[] { nameof(BaseUrl) });

        if (string.IsNullOrWhiteSpace(ApiKey))
            yield return new ValidationResult(SecretHints.ApiKey, new[] { nameof(ApiKey) });
    }
}
