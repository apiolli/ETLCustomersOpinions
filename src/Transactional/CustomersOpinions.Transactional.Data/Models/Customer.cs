namespace CustomersOpinions.Transactional.Data.Models;

public sealed class Customer
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsInferred { get; set; }
}
