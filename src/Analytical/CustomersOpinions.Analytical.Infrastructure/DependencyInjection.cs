using CsvHelper.Configuration;
using CustomersOpinions.Analytical.Application.Abstractions.Extraction;
using CustomersOpinions.Analytical.Application.Abstractions.Reading;
using CustomersOpinions.Analytical.Application.Extraction;
using CustomersOpinions.Analytical.Domain.Staging;
using CustomersOpinions.Analytical.Infrastructure.Configuration;
using CustomersOpinions.Analytical.Infrastructure.Extraction.Api;
using CustomersOpinions.Analytical.Infrastructure.Extraction.Csv;
using CustomersOpinions.Analytical.Infrastructure.Extraction.Database;
using CustomersOpinions.Analytical.Infrastructure.Persistence;
using CustomersOpinions.Analytical.Infrastructure.Reading;
using CustomersOpinions.Analytical.Infrastructure.Sources.Api;
using CustomersOpinions.Analytical.Infrastructure.Sources.Csv;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomersOpinions.Analytical.Infrastructure;

public static class DependencyInjection
{
    private const string ApiKeyHeader = "X-Api-Key";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddValidatedOptions<ConnectionStringsOptions>(services, configuration, ConnectionStringsOptions.SectionName);
        AddValidatedOptions<ExtractionOptions>(services, configuration, ExtractionOptions.SectionName);
        AddValidatedOptions<SurveysSourceOptions>(services, configuration, SurveysSourceOptions.SectionName);
        AddValidatedOptions<SocialCommentsSourceOptions>(services, configuration, SocialCommentsSourceOptions.SectionName);
        foreach (var name in new[] { DatabaseSourceOptions.WebReviews, DatabaseSourceOptions.Customers, DatabaseSourceOptions.Products })
        {
            services.AddOptions<DatabaseSourceOptions>(name)
                .Bind(configuration.GetSection($"Sources:{name}"))
                .ValidateDataAnnotations()
                .ValidateOnStart();
        }

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<ExtractionOptions>>().Value;
            return new ExtractionSettings(options.BatchSize, options.MaxDegreeOfParallelism);
        });

        services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();

        services.AddSingleton<ClassMap<SurveyFileRow>, SurveyFileRowMap>();
        services.AddSingleton<IFileReader<SurveyFileRow>, CsvFileReader<SurveyFileRow>>();
        services.AddSingleton<IFileReaderResolver, FileReaderResolver>();

        AddSocialCommentsClient(services);

        AddSource<StagingSurvey, CsvSurveyExtractor>(services, StagingTableMaps.Surveys, "Surveys (CSV)",
            sp => sp.GetRequiredService<IOptions<SurveysSourceOptions>>().Value.Enabled);
        AddSource<StagingWebReview, WebReviewDatabaseExtractor>(services, StagingTableMaps.WebReviews, "WebReviews (DB)",
            sp => DatabaseEnabled(sp, DatabaseSourceOptions.WebReviews));
        AddSource<StagingCustomer, CustomerDatabaseExtractor>(services, StagingTableMaps.Customers, "Customers (DB)",
            sp => DatabaseEnabled(sp, DatabaseSourceOptions.Customers));
        AddSource<StagingProduct, ProductDatabaseExtractor>(services, StagingTableMaps.Products, "Products (DB)",
            sp => DatabaseEnabled(sp, DatabaseSourceOptions.Products));
        AddSource<StagingSocialComment, SocialCommentApiExtractor>(services, StagingTableMaps.SocialComments, "SocialComments (API)",
            sp => sp.GetRequiredService<IOptions<SocialCommentsSourceOptions>>().Value.Enabled);

        return services;
    }

    private static void AddValidatedOptions<T>(IServiceCollection services, IConfiguration configuration, string section)
        where T : class =>
        services.AddOptions<T>()
            .Bind(configuration.GetSection(section))
            .ValidateDataAnnotations()
            .ValidateOnStart();

    private static bool DatabaseEnabled(IServiceProvider sp, string name) =>
        sp.GetRequiredService<IOptionsMonitor<DatabaseSourceOptions>>().Get(name).Enabled;

    private static void AddSocialCommentsClient(IServiceCollection services)
    {
        var resilienceBuilder = services.AddHttpClient<SocialCommentsApiClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<SocialCommentsSourceOptions>>().Value;
                if (Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseAddress))
                    client.BaseAddress = baseAddress;
                if (!string.IsNullOrWhiteSpace(options.ApiKey))
                    client.DefaultRequestHeaders.Add(ApiKeyHeader, options.ApiKey);
            })
            .AddStandardResilienceHandler();

        services.AddOptions<HttpStandardResilienceOptions>(resilienceBuilder.PipelineName)
            .Configure<IOptions<SocialCommentsSourceOptions>>((resilience, source) =>
            {
                var options = source.Value;
                var total = TimeSpan.FromSeconds(options.TimeoutSeconds);
                var attempt = total < TimeSpan.FromSeconds(10) ? total : TimeSpan.FromSeconds(10);

                resilience.Retry.MaxRetryAttempts = options.RetryCount;
                resilience.Retry.UseJitter = true;
                resilience.TotalRequestTimeout.Timeout = total;
                resilience.AttemptTimeout.Timeout = attempt;
                var sampling = attempt * 2;
                if (resilience.CircuitBreaker.SamplingDuration < sampling)
                    resilience.CircuitBreaker.SamplingDuration = sampling;
            });
    }

    private static void AddSource<TRecord, TExtractor>(IServiceCollection services, StagingTableMap<TRecord> map,
        string displayName, Func<IServiceProvider, bool> isEnabled)
        where TExtractor : class, IExtractor<TRecord>
    {
        services.AddTransient<TExtractor>();
        services.AddTransient<IExtractionSource>(sp => new ExtractionSource<TRecord>(
            sp.GetRequiredService<TExtractor>(),
            new SqlBulkStagingRepository<TRecord>(
                sp.GetRequiredService<IDbConnectionFactory>(), map,
                sp.GetRequiredService<ILogger<SqlBulkStagingRepository<TRecord>>>()),
            new SourceSettings(displayName, isEnabled(sp)),
            sp.GetRequiredService<ExtractionSettings>(),
            sp.GetRequiredService<ILogger<ExtractionSource<TRecord>>>()));
    }
}
