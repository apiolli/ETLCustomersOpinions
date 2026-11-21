namespace CustomersOpinions.Transactional.Data.Models;

public sealed class Product
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public bool IsInferred { get; set; }
}
