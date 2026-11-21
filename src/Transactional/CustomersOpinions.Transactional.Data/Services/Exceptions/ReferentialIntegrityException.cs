namespace CustomersOpinions.Transactional.Data.Services.Exceptions;

public sealed class ReferentialIntegrityException : RecordRejectedException
{
    public ReferentialIntegrityException(string message) : base(message) { }
}
