using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using CustomersOpinions.Analytical.Application.Abstractions.Reading;
using CustomersOpinions.Analytical.Domain.Common;

namespace CustomersOpinions.Analytical.Infrastructure.Reading;

/// <summary>CsvHelper based <see cref="IFileReader{TRow}"/>: streams rows, validating the header first.</summary>
/// <typeparam name="TRow">Row model, mapped by the registered <see cref="ClassMap{TRow}"/>.</typeparam>
public sealed class CsvFileReader<TRow> : IFileReader<TRow>
{
    private const int BufferSize = 64 * 1024;
    private readonly ClassMap<TRow> _map;

    public CsvFileReader(ClassMap<TRow> map) => _map = map;

    public IReadOnlyCollection<string> SupportedExtensions { get; } = new[] { ".csv" };

    public async IAsyncEnumerable<Result<TRow>> ReadAsync(string path,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            yield return Result<TRow>.Failure(ErrorType.SourceUnavailable, $"File not found: {path}");
            yield break;
        }

        var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            TrimOptions = TrimOptions.Trim,
            MissingFieldFound = null,
            HeaderValidated = null,
            BadDataFound = null
        };

        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, useAsync: true);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, BufferSize);
        using var csv = new CsvReader(reader, configuration);
        csv.Context.RegisterClassMap(_map);

        var header = await ReadHeaderAsync(csv);
        if (header.IsFailure)
        {
            yield return Result<TRow>.Failure(ErrorType.SourceUnavailable, header.Message);
            yield break;
        }

        var columns = csv.HeaderRecord!.Length;
        while (await csv.ReadAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return ReadRow(csv, columns);
        }
    }

    private static async Task<Result> ReadHeaderAsync(CsvReader csv)
    {
        if (!await csv.ReadAsync())
            return Result.Failure(ErrorType.SourceUnavailable, "The file is empty.");

        csv.ReadHeader();
        try
        {
            csv.ValidateHeader<TRow>();
            return Result.Success();
        }
        catch (HeaderValidationException ex)
        {
            var missing = ex.InvalidHeaders.Select(h => string.Join("/", h.Names));
            return Result.Failure(ErrorType.SourceUnavailable,
                $"Invalid file structure. Missing columns: {string.Join(", ", missing)}");
        }
    }

    private static Result<TRow> ReadRow(CsvReader csv, int columns)
    {
        var row = csv.Parser.Row;
        if (csv.Parser.Count != columns)
            return Result<TRow>.Failure(ErrorType.Validation,
                $"Row {row}: {csv.Parser.Count} columns found, {columns} expected");

        try
        {
            return Result<TRow>.Success(csv.GetRecord<TRow>());
        }
        catch (CsvHelperException ex)
        {
            return Result<TRow>.Failure(ErrorType.Validation, $"Row {row}: malformed ({ex.Message.Split('\n')[0]})");
        }
    }
}
