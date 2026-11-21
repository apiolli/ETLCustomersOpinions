namespace CustomersOpinions.Transactional.Data.Services.Exceptions;

public abstract class RecordRejectedException : Exception
{
    protected RecordRejectedException(string message) : base(message) { }
}
