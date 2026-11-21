using System.ComponentModel.DataAnnotations;

namespace CustomersOpinions.Analytical.Infrastructure.Configuration;

public sealed class ConnectionStringsOptions
{
    public const string SectionName = "ConnectionStrings";

    [Required(AllowEmptyStrings = false, ErrorMessage = SecretHints.TransactionalDb)]
    public string TransactionalDb { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = SecretHints.AnalyticalDb)]
    public string AnalyticalDb { get; set; } = string.Empty;
}
