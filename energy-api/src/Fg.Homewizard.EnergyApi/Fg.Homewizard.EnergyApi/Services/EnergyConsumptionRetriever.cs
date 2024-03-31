using System.Text.Json;
using Fg.Homewizard.EnergyApi.Clients;
using Fg.Homewizard.EnergyApi.Models;

namespace Fg.Homewizard.EnergyApi.Services
{
    public class EnergyConsumptionRetriever
    {
        private readonly InfluxDbReader _influxDbReader;
        private readonly ILogger<EnergyConsumptionRetriever> _logger;

        public EnergyConsumptionRetriever(InfluxDbReader dbReader, ILogger<EnergyConsumptionRetriever> logger)
        {
            _influxDbReader = dbReader;
            _logger = logger;
        }

        public async Task<IEnumerable<PowerUsage>> GetElectricityConsumptionForPeriodAsync(DateTimeOffset fromDate, DateTimeOffset toDate)
        {
            fromDate = fromDate.AddDays(-1);
            toDate = toDate.AddDays(1);

            string influxQuery =
                "SELECT time, last(totalpower_import_kwh) AS power_import, last(totalpower_export_kwh) AS power_import " +
                " FROM electricity " +
                $" WHERE time >= '{fromDate.Date:yyyy-MM-ddTHH:mm:ss.FFFZ}' and time < '{toDate.Date:yyyy-MM-ddTHH:mm:ss.FFFZ}' " +
                " GROUP BY time(24h)";

            var series = await _influxDbReader.QueryQLAsync(influxQuery);

            var results = new List<PowerUsage>();

            var electricitySerie = series.Results.First().Series.First();

            for (int i = 0; i < electricitySerie.Values.Count(); i++)
            {
                PowerUsage usage = new PowerUsage()
                {
                    Timestamp = ((JsonElement)electricitySerie.Values.ElementAt(i).ElementAt(0)).GetDateTime(),
                    PowerImportReading = ((JsonElement)electricitySerie.Values.ElementAt(i).ElementAt(1)).GetDecimal(),
                    PowerExportReading = ((JsonElement)electricitySerie.Values.ElementAt(i).ElementAt(2)).GetDecimal()
                };

                if (i != 0)
                {
                    usage.PowerExport = usage.PowerExportReading - results[i - 1].PowerExportReading;
                    usage.PowerImport = usage.PowerImportReading - results[i - 1].PowerImportReading;
                }

                results.Add(usage);
            }

            return results;
        }
    }
}
