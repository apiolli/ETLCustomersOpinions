using CustomersOpinions.Transactional.Data.CsvRecords;
using CustomersOpinions.Transactional.Data.DataAccess.Interfaces;
using CustomersOpinions.Transactional.Data.Models;
using CustomersOpinions.Transactional.Data.Services.Reading;
using CustomersOpinions.Transactional.Data.Services.State;
using CustomersOpinions.Transactional.Data.Services.Validation;

namespace CustomersOpinions.Transactional.Data.Services.Loaders;

public sealed class SocialCommentsLoader : FileLoader<SocialCommentCsv, SocialComment>
{
    private readonly IInsertDao<SocialComment> _dao;
    private readonly ICatalogs _catalogs;
    private readonly IReferences _references;

    public SocialCommentsLoader(ICsvReader reader, IExistingRecordsDao existing, IInsertDao<SocialComment> dao,
        ICatalogs catalogs, IReferences references) : base(reader, existing)
    {
        _dao = dao;
        _catalogs = catalogs;
        _references = references;
    }

    public override string FileName => "social_comments.csv";
    protected override Table Table => Table.SocialComment;
    protected override string IdField => "IdComment";
    protected override string? OriginalId(SocialCommentCsv r) => r.CommentId;

    protected override void Validate(SocialCommentCsv r, Validator v)
    {
        v.Format(r.CommentId, "IdComment", Pattern.CommentId);
        if (!string.IsNullOrWhiteSpace(r.CustomerId))
        {
            v.Format(r.CustomerId, "IdCliente", Pattern.CustomerId);
        }

        v.Format(r.ProductId, "IdProducto", Pattern.ProductId);
        v.Required(r.Source, "Fuente", 50);
        v.Date(r.Date, "Fecha");
        v.Required(r.Comment, "Comentario", Validator.MaxComment);
    }

    protected override SocialComment Transform(SocialCommentCsv r) => new()
    {
        CommentId = r.CommentId!.Trim(),
        CustomerId = Conversions.StripOptionalPrefix(r.CustomerId),
        ProductId = Conversions.StripPrefix(r.ProductId!),
        SocialNetworkId = _catalogs.GetId(Catalog.SocialNetwork, r.Source!),
        Date = Conversions.Date(r.Date!),
        Comment = r.Comment!.Trim()
    };

    protected override string Id(SocialComment c) => c.CommentId;

    protected override string ContentKey(SocialComment c) =>
        OpinionKey.Create(c.CustomerId, c.ProductId, c.Date, c.Comment);

    protected override bool Insert(SocialComment comment, LoadResult result)
    {
        if (comment.CustomerId.HasValue)
            _references.RequireCustomer(comment.CustomerId.Value);
        _references.RequireProduct(comment.ProductId);
        return _dao.Insert(comment);
    }
}
