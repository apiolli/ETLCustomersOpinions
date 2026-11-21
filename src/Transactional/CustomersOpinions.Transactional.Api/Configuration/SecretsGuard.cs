namespace CustomersOpinions.Transactional.Api.Configuration;

/// <summary>Checks the required secrets at startup and explains how to configure the missing ones.</summary>
internal static class SecretsGuard
{
    private const string ProjectPath = "src/Transactional/CustomersOpinions.Transactional.Api";

    private static readonly (string Key, string Example)[] RequiredSecrets =
    {
        ("ConnectionStrings:TransactionalDb",
            "Server=.;Database=OpinionesClientesDB;Integrated Security=True;TrustServerCertificate=True"),
        ("Security:ApiKey", "<key>")
    };

    /// <summary>Returns the instructions for every missing secret (empty when all are present).</summary>
    public static IReadOnlyList<string> FindMissing(IConfiguration configuration) =>
        RequiredSecrets
            .Where(s => string.IsNullOrWhiteSpace(configuration[s.Key]))
            .Select(s => $"  dotnet user-secrets set \"{s.Key}\" \"{s.Example}\" --project {ProjectPath}")
            .ToList();
}
