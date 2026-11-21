namespace CustomersOpinions.Data.Services.Exceptions;

public class ValidationException : RegistroRechazadoException
{
    public ValidationException(string mensaje) : base(mensaje) { }
    public ValidationException(IEnumerable<string> errores) : base(string.Join("; ", errores)) { }
}
