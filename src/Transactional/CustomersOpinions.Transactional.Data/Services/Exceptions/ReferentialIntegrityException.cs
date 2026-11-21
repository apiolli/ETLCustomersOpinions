namespace CustomersOpinions.Transactional.Data.Services.Exceptions;

/// <summary>A record references a customer or product that does not exist.</summary>
public sealed class ReferentialIntegrityException : RecordRejectedException
{
    public ReferentialIntegrityException(string message) : base(message) { }
}
