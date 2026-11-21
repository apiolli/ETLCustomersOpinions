namespace CustomersOpinions.Data.Services.Exceptions;

public abstract class RegistroRechazadoException : Exception
{
    protected RegistroRechazadoException(string mensaje) : base(mensaje) { }
}
