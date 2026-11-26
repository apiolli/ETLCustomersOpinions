namespace CustomersOpinions.Analytical.Infrastructure.Loading.Base;

public sealed record LoadCounts(int Read, int Inserted, int Updated, int Unchanged, int Rejected = 0, string Message = "");

public sealed class ScdTally
{
    public const string ActionParameter = "@Accion";

    public int Inserted { get; private set; }

    public int Updated { get; private set; }

    public int Unchanged { get; private set; }

    public void Add(object? action)
    {
        switch (action as string)
        {
            case "Insertado":
                Inserted++;
                break;
            case "Actualizado":
                Updated++;
                break;
            default:
                Unchanged++;
                break;
        }
    }

    public LoadCounts ToCounts(int read, int rejected, string message = "") =>
        new(read, Inserted, Updated, Unchanged, rejected, message);
}
