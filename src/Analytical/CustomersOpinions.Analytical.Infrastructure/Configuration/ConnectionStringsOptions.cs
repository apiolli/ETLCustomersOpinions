using System.ComponentModel.DataAnnotations;

namespace CustomersOpinions.Analytical.Infrastructure.Configuration;

/// <summary>Section <c>ConnectionStrings</c>. Values come from User Secrets or environment variables.</summary>
public sealed class ConnectionStringsOptions
{
    public const string SectionName = "ConnectionStrings";

    /// <summary>Source database <c>OpinionesClientesDB</c>.</summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = SecretHints.TransactionalDb)]
    public string TransactionalDb { get; set; } = string.Empty;

    /// <summary>Data warehouse <c>OpinionesClientesDW</c> (staging tables).</summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = SecretHints.AnalyticalDb)]
    public string AnalyticalDb { get; set; } = string.Empty;
}
