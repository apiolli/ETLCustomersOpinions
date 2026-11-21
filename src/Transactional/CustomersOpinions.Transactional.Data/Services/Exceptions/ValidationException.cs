namespace CustomersOpinions.Transactional.Data.Services.Exceptions;

public sealed class ValidationException : RecordRejectedException
{
    public ValidationException(string message) : base(message) { }

    public ValidationException(IEnumerable<string> errors) : base(string.Join("; ", errors)) { }
}
