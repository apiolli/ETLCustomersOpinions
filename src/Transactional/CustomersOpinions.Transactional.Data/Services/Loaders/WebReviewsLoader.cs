using CustomersOpinions.Transactional.Data.CsvRecords;
using CustomersOpinions.Transactional.Data.DataAccess.Interfaces;
using CustomersOpinions.Transactional.Data.Models;
using CustomersOpinions.Transactional.Data.Services.Reading;
using CustomersOpinions.Transactional.Data.Services.State;
using CustomersOpinions.Transactional.Data.Services.Validation;

namespace CustomersOpinions.Transactional.Data.Services.Loaders;

public sealed class WebReviewsLoader : FileLoader<WebReviewCsv, WebReview>
{
    private readonly IInsertDao<WebReview> _dao;
    private readonly IReferences _references;

    public WebReviewsLoader(ICsvReader reader, IExistingRecordsDao existing, IInsertDao<WebReview> dao,
        IReferences references) : base(reader, existing)
    {
        _dao = dao;
        _references = references;
    }

    public override string FileName => "web_reviews.csv";
    protected override Table Table => Table.WebReview;
    protected override string IdField => "IdReview";
    protected override string? OriginalId(WebReviewCsv r) => r.ReviewId;

    protected override void Validate(WebReviewCsv r, Validator v)
    {
        v.Format(r.ReviewId, "IdReview", Pattern.ReviewId);
        v.Format(r.CustomerId, "IdCliente", Pattern.CustomerId);
        v.Format(r.ProductId, "IdProducto", Pattern.ProductId);
        v.Date(r.Date, "Fecha");
        v.Required(r.Comment, "Comentario", Validator.MaxComment);
        v.Range(r.Rating, "Rating", 1, 5);
    }

    protected override WebReview Transform(WebReviewCsv r) => new()
    {
        ReviewId = r.ReviewId!.Trim(),
        CustomerId = Conversions.StripPrefix(r.CustomerId!),
        ProductId = Conversions.StripPrefix(r.ProductId!),
        Date = Conversions.Date(r.Date!),
        Comment = r.Comment!.Trim(),
        Rating = Conversions.Byte(r.Rating!)
    };

    protected override string Id(WebReview w) => w.ReviewId;

    protected override string ContentKey(WebReview w) =>
        OpinionKey.Create(w.CustomerId, w.ProductId, w.Date, w.Comment);

    protected override bool Insert(WebReview review, LoadResult result)
    {
        _references.RequireCustomer(review.CustomerId);
        _references.RequireProduct(review.ProductId);
        return _dao.Insert(review);
    }
}
