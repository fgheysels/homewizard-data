namespace Fg.Homewizard.EnergyApi.Models
{
    public class PowerUsage
    {
        public DateTime Timestamp { get; set; }
        public decimal PowerImportReading { get; set; }
        public decimal PowerExportReading { get; set; }
        public decimal PowerImport { get; set; }
        public decimal PowerExport { get; set; }
    }
}
