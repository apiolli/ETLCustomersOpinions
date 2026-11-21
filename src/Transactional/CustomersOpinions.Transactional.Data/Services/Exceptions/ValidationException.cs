namespace CustomersOpinions.Transactional.Data.Services.Exceptions;

/// <summary>A record failed one or more validation rules.</summary>
public sealed class ValidationException : RecordRejectedException
{
    public ValidationException(string message) : base(message) { }

    public ValidationException(IEnumerable<string> errors) : base(string.Join("; ", errors)) { }
}
