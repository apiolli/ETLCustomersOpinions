namespace CustomersOpinions.Transactional.Data.Services.Reading;

public sealed record CsvRow<T>(int Number, T? Record, string? Error);
