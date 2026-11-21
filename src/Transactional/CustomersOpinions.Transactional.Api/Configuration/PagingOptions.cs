using System.ComponentModel.DataAnnotations;

namespace CustomersOpinions.Transactional.Api.Configuration;

/// <summary>Paging limits of the social comments endpoint (section <c>Paging</c>).</summary>
public sealed class PagingOptions
{
    public const string SectionName = "Paging";

    [Range(1, 100_000)]
    public int DefaultPageSize { get; set; } = 500;

    [Range(1, 100_000)]
    public int MaxPageSize { get; set; } = 1000;
}
