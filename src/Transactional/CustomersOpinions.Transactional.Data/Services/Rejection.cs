namespace CustomersOpinions.Transactional.Data.Services;

public sealed record Rejection(string File, int Row, string? RecordId, string Reason);
