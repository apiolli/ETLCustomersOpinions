namespace CustomersOpinions.Data.Services
{
    public class ResumenCarga
    {
        public bool Simulacion { get; init; }
        public List<ResultadoCarga> Archivos { get; } = new();
        public int Procesados => Archivos.Sum(a => a.Procesados);
        public int Insertados => Archivos.Sum(a => a.Insertados);
        public int Rechazados => Archivos.Sum(a => a.Rechazados);
        public int ClientesProvisionales => Archivos.Sum(a => a.ClientesProvisionales);
        public int ProductosProvisionales => Archivos.Sum(a => a.ProductosProvisionales);
        public IEnumerable<Rechazo> Rechazos => Archivos.SelectMany(a => a.Rechazos);
    }
}
