namespace CustomersOpinions.Transactional.Data.Services;

/// <summary>A rejected CSV record and the reason.</summary>
public sealed record Rejection(string File, int Row, string? RecordId, string Reason);
