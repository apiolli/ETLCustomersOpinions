using CsvHelper.Configuration;

namespace CustomersOpinions.Analytical.Infrastructure.Sources.Csv;

public sealed class SurveyFileRowMap : ClassMap<SurveyFileRow>
{
    public SurveyFileRowMap()
    {
        Map(r => r.OpinionId).Name("IdOpinion");
        Map(r => r.CustomerId).Name("IdCliente");
        Map(r => r.ProductId).Name("IdProducto");
        Map(r => r.Date).Name("Fecha");
        Map(r => r.Comment).Name("Comentario");
        Map(r => r.Classification).Name("Clasificación");
        Map(r => r.SatisfactionScore).Name("PuntajeSatisfacción");
        Map(r => r.Source).Name("Fuente");
    }
}
