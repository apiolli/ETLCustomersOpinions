using CustomersOpinions.Transactional.Data.CsvRecords;
using CustomersOpinions.Transactional.Data.DataAccess.Interfaces;
using CustomersOpinions.Transactional.Data.Models;
using CustomersOpinions.Transactional.Data.Services.Reading;
using CustomersOpinions.Transactional.Data.Services.State;
using CustomersOpinions.Transactional.Data.Services.Validation;

namespace CustomersOpinions.Transactional.Data.Services.Loaders;

public sealed class DataSourcesLoader : FileLoader<DataSourceCsv, DataSource>
{
    private readonly IInsertDao<DataSource> _dao;
    private readonly ICatalogs _catalogs;

    public DataSourcesLoader(ICsvReader reader, IExistingRecordsDao existing, IInsertDao<DataSource> dao,
        ICatalogs catalogs) : base(reader, existing)
    {
        _dao = dao;
        _catalogs = catalogs;
    }

    public override string FileName => "fuente_datos.csv";
    protected override Table Table => Table.DataSource;
    protected override string IdField => "IdFuente";
    protected override string? OriginalId(DataSourceCsv r) => r.SourceId;

    protected override void Validate(DataSourceCsv r, Validator v)
    {
        v.Format(r.SourceId, "IdFuente", Pattern.SourceId);
        v.Required(r.SourceType, "TipoFuente", 50);
        v.Date(r.LoadDate, "FechaCarga");
    }

    protected override DataSource Transform(DataSourceCsv r) => new()
    {
        SourceId = r.SourceId!.Trim(),
        SourceTypeId = _catalogs.GetId(Catalog.SourceType, r.SourceType!),
        LoadDate = Conversions.Date(r.LoadDate!)
    };

    protected override string Id(DataSource d) => d.SourceId;

    protected override bool Insert(DataSource source, LoadResult result) => _dao.Insert(source);
}
