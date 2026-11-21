using CustomersOpinions.Transactional.Api.Configuration;
using CustomersOpinions.Transactional.Api.Data;
using CustomersOpinions.Transactional.Api.Endpoints;
using CustomersOpinions.Transactional.Api.Security;

var builder = WebApplication.CreateBuilder(args);

var missingSecrets = SecretsGuard.FindMissing(builder.Configuration);
if (missingSecrets.Count > 0)
{
    Console.Error.WriteLine("CustomersOpinions API cannot start: required secrets are not configured.");
    Console.Error.WriteLine("Run the following command(s) and start the API again:");
    foreach (var line in missingSecrets)
        Console.Error.WriteLine(line);
    return 1;
}

builder.Services.AddOptions<PagingOptions>()
    .Bind(builder.Configuration.GetSection(PagingOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<ISocialCommentRepository, SqlSocialCommentRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<ApiKeyMiddleware>();
app.MapSocialCommentEndpoints();

await app.RunAsync();
return 0;
