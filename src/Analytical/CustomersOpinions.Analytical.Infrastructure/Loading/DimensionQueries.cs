namespace CustomersOpinions.Analytical.Infrastructure.Loading;

internal static class DimensionQueries
{
    public const string SeedMembers = "dbo.usp_SeedDimensionMembers";
    public const string LoadTime = "dbo.usp_LoadDimTiempo";
    public const string LoadCustomer = "dbo.usp_LoadDimCliente";
    public const string LoadProduct = "dbo.usp_LoadDimProducto";
    public const string LoadChannel = "dbo.usp_LoadDimCanal";
    public const string LoadEvent = "dbo.usp_LoadDimEvento";
    public const string LoadKeyword = "dbo.usp_LoadDimPalabraClave";

    public const string StagingClassifications = """
        select distinct ltrim(rtrim(Classification))
        from Staging.Surveys
        where nullif(ltrim(rtrim(Classification)), '') is not null;
        """;

    public const string ExistingClassifications = "select Nombre from dbo.DimClasificacion;";

    public const string StagingDateRange = """
        select min(d.Fecha), max(d.Fecha), sum(case when d.Fecha is null then 1 else 0 end)
        from (
            select nullif(ltrim(rtrim(SurveyDate)), '') as Texto, try_convert(date, SurveyDate, 23) as Fecha from Staging.Surveys
            union all
            select nullif(ltrim(rtrim(ReviewDate)), ''), try_convert(date, ReviewDate, 23) from Staging.WebReviews
            union all
            select nullif(ltrim(rtrim(CommentDate)), ''), try_convert(date, CommentDate, 23) from Staging.SocialComments
        ) d
        where d.Texto is not null;
        """;

    public const string StagingCustomers = "select CustomerId, Name, Email, IsInferred from Staging.Customers;";

    public const string StagingProducts = "select ProductId, Name, Category, IsInferred from Staging.Products;";

    public const string StagingChannels = """
        select 'Survey', null from Staging.Surveys having count(*) > 0
        union all
        select 'WebReview', null from Staging.WebReviews having count(*) > 0
        union all
        select distinct 'Social', ltrim(rtrim(Platform)) from Staging.SocialComments
        where nullif(ltrim(rtrim(Platform)), '') is not null;
        """;

    public const string ExistingEvents = "select Nombre, TipoEvento, FechaInicio, FechaFin from dbo.DimEvento;";

    public const string StagingComments = """
        select Comment from Staging.Surveys
        union all
        select Comment from Staging.WebReviews
        union all
        select Comment from Staging.SocialComments;
        """;

    public const string ExistingKeywords = "select Palabra from dbo.DimPalabraClave;";
}
