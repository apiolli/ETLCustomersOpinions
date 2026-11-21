namespace CustomersOpinions.Transactional.Data.Services.Exceptions;

/// <summary>Base of the exceptions that reject a single record without stopping the file.</summary>
public abstract class RecordRejectedException : Exception
{
    protected RecordRejectedException(string message) : base(message) { }
}
