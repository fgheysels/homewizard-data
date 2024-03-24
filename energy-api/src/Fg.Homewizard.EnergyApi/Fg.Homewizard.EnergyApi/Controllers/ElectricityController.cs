using Fg.Homewizard.EnergyApi.Clients;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fg.Homewizard.EnergyApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElectricityController : ControllerBase
    {
        private readonly InfluxDbReader _influxInfluxDbReader;
        private readonly ILogger<ElectricityController> _logger;

        public ElectricityController(InfluxDbReader influxDbReader, ILogger<ElectricityController> logger)
        {
            _influxInfluxDbReader = influxDbReader;
            _logger = logger;
        }

        [HttpGet("daily")]
        public async Task<IActionResult> GetDailyElectricityData(DateTimeOffset fromDate, DateTimeOffset toDate)
        {
            fromDate = fromDate.AddDays(-1);

            string influxQuery =
                "SELECT time, last(totalpower_import_kwh) AS power_import, last(totalpower_export_kwh) AS power_import " +
                " FROM electricity " +
                $" WHERE time >= '{fromDate.Date:yyyy-MM-ddTHH:mm:ss.FFFZ}' and time <= '{toDate.Date:yyyy-MM-ddTHH:mm:ss.FFFZ}' " +
                " GROUP BY time(24h)";
            _logger.LogInformation(influxQuery);
            var results = await _influxInfluxDbReader.QueryQLAsync(influxQuery);

            return Ok(results);
        }
    }
}
