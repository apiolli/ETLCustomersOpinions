using CustomersOpinions.Transactional.Data.DataAccess;
using CustomersOpinions.Transactional.Data.Services.Loaders;
using CustomersOpinions.Transactional.Data.Services.Reading;
using CustomersOpinions.Transactional.Data.Services.State;

namespace CustomersOpinions.Transactional.Data.Services;

/// <summary>Composes the DAOs, state and loaders, and runs the load of the six CSV files.</summary>
public static class LoadFactory
{
    public static LoadSummary Run(string connectionString, string csvFolder, bool dryRun, bool createInferredRecords)
    {
        using var session = new DbSession(connectionString, dryRun);

        var reader = new CsvReaderService();
        var existing = new ExistingRecordsDao(session);
        var customerDao = new CustomerDao(session);
        var productDao = new ProductDao(session);
        var catalogs = new CatalogCache(new CatalogDao(session));
        var references = new LoadReferences(customerDao, productDao);
        var inferred = new InferredRecords(customerDao, productDao, catalogs, references, createInferredRecords);

        var loaders = new IFileLoader[]
        {
            new CustomersLoader(reader, existing, customerDao, references),
            new ProductsLoader(reader, existing, productDao, catalogs, references),
            new DataSourcesLoader(reader, existing, new DataSourceDao(session), catalogs),
            new SurveysLoader(reader, existing, new SurveyDao(session), catalogs, inferred),
            new SocialCommentsLoader(reader, existing, new SocialCommentDao(session), catalogs, references),
            new WebReviewsLoader(reader, existing, new WebReviewDao(session), references)
        };

        var states = new ILoadState[] { catalogs, references };
        return new LoadService(session, states, loaders).Run(csvFolder);
    }
}
