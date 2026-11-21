namespace CustomersOpinions.Analytical.Infrastructure.Configuration;

/// <summary>Validation messages that tell how to configure each secret (never its value).</summary>
internal static class SecretHints
{
    private const string Project = "--project src/Analytical/CustomersOpinions.Analytical.Worker";

    public const string TransactionalDb =
        "Missing ConnectionStrings:TransactionalDb. Run: dotnet user-secrets set \"ConnectionStrings:TransactionalDb\" \"Server=.;Database=OpinionesClientesDB;Integrated Security=True;TrustServerCertificate=True\" " + Project;

    public const string AnalyticalDb =
        "Missing ConnectionStrings:AnalyticalDb. Run: dotnet user-secrets set \"ConnectionStrings:AnalyticalDb\" \"Server=.;Database=OpinionesClientesDW;Integrated Security=True;TrustServerCertificate=True\" " + Project;

    public const string ApiKey =
        "Missing Sources:SocialComments:ApiKey. Run: dotnet user-secrets set \"Sources:SocialComments:ApiKey\" \"<key>\" " + Project;
}
