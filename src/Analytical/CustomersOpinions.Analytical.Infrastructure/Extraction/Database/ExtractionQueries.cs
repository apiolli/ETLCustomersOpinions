namespace CustomersOpinions.Analytical.Infrastructure.Extraction.Database;

internal static class ExtractionQueries
{
    public const string WebReviewsView = "dbo.vw_ExtractWebReviews";
    public const string CustomersView = "dbo.vw_ExtractCustomers";
    public const string ProductsView = "dbo.vw_ExtractProducts";

    public const string WebReviews =
        "select IdReview, IdCliente, IdProducto, Fecha, Comentario, Rating from " + WebReviewsView + " order by IdReview;";

    public const string Customers =
        "select IdCliente, Nombre, Email, EsInferido from " + CustomersView + " order by IdCliente;";

    public const string Products =
        "select IdProducto, Nombre, Categoria, EsInferido from " + ProductsView + " order by IdProducto;";
}
