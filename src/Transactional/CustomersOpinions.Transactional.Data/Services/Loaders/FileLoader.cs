using CustomersOpinions.Transactional.Data.DataAccess.Interfaces;
using CustomersOpinions.Transactional.Data.Models;
using CustomersOpinions.Transactional.Data.Services.Exceptions;
using CustomersOpinions.Transactional.Data.Services.Reading;
using CustomersOpinions.Transactional.Data.Services.Validation;
using Microsoft.Data.SqlClient;

namespace CustomersOpinions.Transactional.Data.Services.Loaders;

/// <summary>
/// Template for loading one CSV file: read, validate, transform, detect duplicates and insert.
/// A bad record is rejected and never stops the file.
/// </summary>
public abstract class FileLoader<TCsv, TModel> : IFileLoader
{
    private static readonly HashSet<int> RecordErrors = new() { 547, 2601, 2627, 2628, 8152 };
    private readonly ICsvReader _reader;
    private readonly IExistingRecordsDao _existing;

    protected FileLoader(ICsvReader reader, IExistingRecordsDao existing)
    {
        _reader = reader;
        _existing = existing;
    }

    public abstract string FileName { get; }
    protected abstract Table Table { get; }
    protected abstract string IdField { get; }
    protected abstract string? OriginalId(TCsv record);
    protected abstract void Validate(TCsv record, Validator validator);
    protected abstract TModel Transform(TCsv record);
    protected abstract string Id(TModel entity);
    protected virtual string? ContentKey(TModel entity) => null;
    protected abstract bool Insert(TModel entity, LoadResult result);

    public void Process(string path, LoadResult result)
    {
        var duplicates = new Duplicates(
            new HashSet<string>(_existing.GetIds(Table), StringComparer.OrdinalIgnoreCase),
            () => _existing.GetContentKeys(Table));

        foreach (var row in _reader.Read<TCsv>(path))
        {
            result.Processed++;
            if (row.Error != null)
            {
                result.Reject(row.Number, null, row.Error);
                continue;
            }

            try
            {
                ProcessRecord(row.Record!, duplicates, result);
            }
            catch (RecordRejectedException ex)
            {
                result.Reject(row.Number, OriginalId(row.Record!), ex.Message);
            }
            catch (SqlException ex) when (RecordErrors.Contains(ex.Number))
            {
                result.Reject(row.Number, OriginalId(row.Record!), $"Rejected by the database: {ex.Message}");
            }
        }
    }

    private void ProcessRecord(TCsv record, Duplicates duplicates, LoadResult result)
    {
        var validator = new Validator();
        Validate(record, validator);
        validator.ThrowIfErrors();

        var entity = Transform(record);
        var id = Id(entity);
        var key = ContentKey(entity);
        duplicates.Check(IdField, id, key);

        if (!Insert(entity, result))
            throw new ValidationException($"{IdField} {id} already exists in the database");

        duplicates.RegisterInserted(key);
        result.Inserted++;
    }

    private sealed class Duplicates
    {
        private readonly HashSet<string> _idsInFile = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _idsInDatabase;
        private readonly Func<IReadOnlyList<string>> _readKeysInDatabase;
        private HashSet<string>? _keys;

        public Duplicates(HashSet<string> idsInDatabase, Func<IReadOnlyList<string>> readKeysInDatabase)
        {
            _idsInDatabase = idsInDatabase;
            _readKeysInDatabase = readKeysInDatabase;
        }

        public void Check(string idField, string id, string? key)
        {
            if (!_idsInFile.Add(id))
                throw new ValidationException($"{idField} {id} repeated in the file");

            if (_idsInDatabase.Contains(id))
                throw new ValidationException($"{idField} {id} already exists in the database");

            if (key != null && Keys().Contains(key))
                throw new ValidationException("Duplicate opinion: same customer, product, date and comment as another record");
        }

        public void RegisterInserted(string? key)
        {
            if (key != null)
                Keys().Add(key);
        }

        private HashSet<string> Keys() =>
            _keys ??= new HashSet<string>(_readKeysInDatabase(), StringComparer.OrdinalIgnoreCase);
    }
}
