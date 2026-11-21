using CustomersOpinions.Analytical.Application.Abstractions.Reading;
using CustomersOpinions.Analytical.Domain.Staging;
using CustomersOpinions.Analytical.Infrastructure.Configuration;
using CustomersOpinions.Analytical.Infrastructure.Extraction.Base;
using CustomersOpinions.Analytical.Infrastructure.Persistence;
using CustomersOpinions.Analytical.Infrastructure.Sources.Csv;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomersOpinions.Analytical.Infrastructure.Extraction.Csv;

public sealed class CsvSurveyExtractor : FileExtractorBase<SurveyFileRow, StagingSurvey>
{
    public CsvSurveyExtractor(IFileReaderResolver resolver, IOptions<SurveysSourceOptions> options,
        TimeProvider timeProvider, ILogger<CsvSurveyExtractor> logger)
        : base(resolver, options.Value.ResolveFilePath(), StagingTableMaps.Surveys, timeProvider, logger)
    {
    }

    protected override StagingSurvey Map(SurveyFileRow row) => new()
    {
        SurveyId = row.OpinionId,
        CustomerId = row.CustomerId,
        ProductId = row.ProductId,
        SurveyDate = row.Date,
        Comment = row.Comment,
        Classification = row.Classification,
        SatisfactionScore = row.SatisfactionScore,
        Source = row.Source
    };
}
