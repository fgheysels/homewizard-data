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

            if (series.ContainsData)
            {
                var electricitySerie = series.Results.First().Series.First();

                for (int i = 0; i < electricitySerie.Values.Count(); i++)
                {
                    var dataEntry = electricitySerie.Values.ElementAt(i);

                    var dateTimeObject = dataEntry.ElementAt(0);
                    var powerImportObject = dataEntry.ElementAt(1);
                    var powerExportObject = dataEntry.ElementAt(2);

                    PowerUsage usage = new PowerUsage
                    {
                        Timestamp = ((JsonElement)dateTimeObject).GetDateTime(),
                        PowerImportReading = powerImportObject != null ? ((JsonElement)powerImportObject).GetDecimal() : null,
                        PowerExportReading = powerExportObject != null ? ((JsonElement)powerExportObject).GetDecimal() : null
                    };

                    if (i != 0 &&
                        usage.PowerImportReading != null && usage.PowerExportReading != null &&
                        results[i - 1].PowerImportReading != null && results[i - 1].PowerExportReading != null)
                    {
                        usage.PowerExport = usage.PowerExportReading.Value - results[i - 1].PowerExportReading.Value;
                        usage.PowerImport = usage.PowerImportReading.Value - results[i - 1].PowerImportReading.Value;
                    }

                    results.Add(usage);

                }
            }

            return results;
        }
    }
}
