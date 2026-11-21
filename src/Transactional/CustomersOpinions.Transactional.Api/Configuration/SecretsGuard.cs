namespace CustomersOpinions.Transactional.Api.Configuration;

internal static class SecretsGuard
{
    private const string ProjectPath = "src/Transactional/CustomersOpinions.Transactional.Api";

    private static readonly (string Key, string Example)[] RequiredSecrets =
    {
        ("ConnectionStrings:TransactionalDb",
            "Server=.;Database=OpinionesClientesDB;Integrated Security=True;TrustServerCertificate=True"),
        ("Security:ApiKey", "<key>")
    };

    public static IReadOnlyList<string> FindMissing(IConfiguration configuration) =>
        RequiredSecrets
            .Where(s => string.IsNullOrWhiteSpace(configuration[s.Key]))
            .Select(s => $"  dotnet user-secrets set \"{s.Key}\" \"{s.Example}\" --project {ProjectPath}")
            .ToList();
}
