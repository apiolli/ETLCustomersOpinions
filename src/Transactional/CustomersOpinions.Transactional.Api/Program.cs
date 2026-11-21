using CustomersOpinions.Transactional.Api.Configuration;
using CustomersOpinions.Transactional.Api.Data;
using CustomersOpinions.Transactional.Api.Endpoints;
using CustomersOpinions.Transactional.Api.Security;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

const string ApiKeyScheme = "ApiKey";

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
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "CustomersOpinions Social Comments API", Version = "v1" });
    options.AddSecurityDefinition(ApiKeyScheme, new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = ApiKeyMiddleware.HeaderName
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = ApiKeyScheme } }] =
            Array.Empty<string>()
    });
});
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options => options.RouteTemplate = "openapi/{documentName}.json");
    app.MapScalarApiReference(options => options.WithTitle("CustomersOpinions Social Comments API"));
}

app.UseHttpsRedirection();
app.UseMiddleware<ApiKeyMiddleware>();
app.MapSocialCommentEndpoints();

await app.RunAsync();
return 0;
