namespace CustomersOpinions.Data.Services.Exceptions;

public class ReferentialIntegrityException : RegistroRechazadoException
{
    public ReferentialIntegrityException(string mensaje) : base(mensaje) { }
}
