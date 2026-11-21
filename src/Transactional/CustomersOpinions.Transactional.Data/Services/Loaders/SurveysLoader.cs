using CustomersOpinions.Transactional.Data.CsvRecords;
using CustomersOpinions.Transactional.Data.DataAccess.Interfaces;
using CustomersOpinions.Transactional.Data.Models;
using CustomersOpinions.Transactional.Data.Services.Reading;
using CustomersOpinions.Transactional.Data.Services.State;
using CustomersOpinions.Transactional.Data.Services.Validation;

namespace CustomersOpinions.Transactional.Data.Services.Loaders;

public sealed class SurveysLoader : FileLoader<SurveyCsv, Survey>
{
    private static readonly Dictionary<string, (int Min, int Max)> RangeByClassification =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Negativa"] = (1, 2),
            ["Neutra"] = (3, 3),
            ["Positiva"] = (4, 5)
        };

    private readonly IInsertDao<Survey> _dao;
    private readonly ICatalogs _catalogs;
    private readonly IInferredRecords _inferred;

    public SurveysLoader(ICsvReader reader, IExistingRecordsDao existing, IInsertDao<Survey> dao,
        ICatalogs catalogs, IInferredRecords inferred) : base(reader, existing)
    {
        _dao = dao;
        _catalogs = catalogs;
        _inferred = inferred;
    }

    public override string FileName => "surveys_part1.csv";
    protected override Table Table => Table.Survey;
    protected override string IdField => "IdOpinion";
    protected override string? OriginalId(SurveyCsv r) => r.OpinionId;

    protected override void Validate(SurveyCsv r, Validator v)
    {
        v.PositiveInteger(r.OpinionId, "IdOpinion");
        v.PositiveInteger(r.CustomerId, "IdCliente");
        v.PositiveInteger(r.ProductId, "IdProducto");
        v.Date(r.Date, "Fecha");
        v.Required(r.Comment, "Comentario", Validator.MaxComment);
        v.Required(r.Source, "Fuente", 50);
        var score = v.Range(r.SatisfactionScore, "PuntajeSatisfaccion", 1, 5);

        if (string.IsNullOrWhiteSpace(r.Classification))
        {
            v.Add("Clasificacion is required");
        }
        else if (!RangeByClassification.TryGetValue(r.Classification, out var range))
        {
            v.Add($"Unknown Clasificacion '{r.Classification}' (Positiva, Neutra or Negativa)");
        }
        else if (score.HasValue && (score < range.Min || score > range.Max))
        {
            v.Add($"Clasificacion '{r.Classification}' inconsistent with score {score}");
        }
    }

    protected override Survey Transform(SurveyCsv r) => new()
    {
        OpinionId = Conversions.Integer(r.OpinionId!),
        CustomerId = Conversions.Integer(r.CustomerId!),
        ProductId = Conversions.Integer(r.ProductId!),
        Date = Conversions.Date(r.Date!),
        Comment = r.Comment!.Trim(),
        ClassificationId = _catalogs.GetId(Catalog.Classification, r.Classification!),
        SatisfactionScore = Conversions.Byte(r.SatisfactionScore!)
    };

    protected override string Id(Survey s) => s.OpinionId.ToString();

    protected override string ContentKey(Survey s) =>
        OpinionKey.Create(s.CustomerId, s.ProductId, s.Date, s.Comment);

    protected override bool Insert(Survey survey, LoadResult result)
    {
        _inferred.EnsureCustomer(survey.CustomerId, result);
        _inferred.EnsureProduct(survey.ProductId, result);
        return _dao.Insert(survey);
    }
}
