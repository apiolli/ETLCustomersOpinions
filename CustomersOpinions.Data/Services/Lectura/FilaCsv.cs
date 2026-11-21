namespace CustomersOpinions.Data.Services.Lectura
{
    public record FilaCsv<T>(int Numero, T? Registro, string? Error);

}
