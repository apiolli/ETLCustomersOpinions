using System.Globalization;

namespace CustomersOpinions.Transactional.Data.Services;

public static class OpinionKey
{
    public static string Create(int? customerId, int productId, DateTime date, string comment) =>
        string.Create(CultureInfo.InvariantCulture, $"{customerId}|{productId}|{date:yyyy-MM-dd}|{comment}");
}
