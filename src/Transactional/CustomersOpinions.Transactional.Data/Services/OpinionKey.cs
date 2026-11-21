using System.Globalization;

namespace CustomersOpinions.Transactional.Data.Services;

/// <summary>Content key used to detect the same opinion loaded twice under different ids.</summary>
public static class OpinionKey
{
    public static string Create(int? customerId, int productId, DateTime date, string comment) =>
        string.Create(CultureInfo.InvariantCulture, $"{customerId}|{productId}|{date:yyyy-MM-dd}|{comment}");
}
