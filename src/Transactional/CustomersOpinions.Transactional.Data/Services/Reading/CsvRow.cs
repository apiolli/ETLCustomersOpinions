namespace CustomersOpinions.Transactional.Data.Services.Reading;

/// <summary>One CSV line: either a parsed record or the reason it could not be parsed.</summary>
public sealed record CsvRow<T>(int Number, T? Record, string? Error);
