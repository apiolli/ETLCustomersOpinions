using System.Security.Cryptography;
using System.Text;

namespace CustomersOpinions.Transactional.Api.Security;

/// <summary>
/// Requires the <c>X-Api-Key</c> header on every request except <c>/health</c> and <c>/swagger</c>.
/// The key is compared in constant time.
/// </summary>
public sealed class ApiKeyMiddleware
{
    public const string HeaderName = "X-Api-Key";
    private readonly RequestDelegate _next;
    private readonly byte[] _expectedKey;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        var key = configuration["Security:ApiKey"];
        if (string.IsNullOrWhiteSpace(key))
            throw new InvalidOperationException("Missing 'Security:ApiKey'.");
        _expectedKey = Encoding.UTF8.GetBytes(key);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsAnonymousPath(context.Request.Path))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(HeaderName, out var provided) ||
            !CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(provided.ToString()), _expectedKey))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Missing or invalid API key." });
            return;
        }

        await _next(context);
    }

    private static bool IsAnonymousPath(PathString path) =>
        path.StartsWithSegments("/health") || path.StartsWithSegments("/swagger");
}
