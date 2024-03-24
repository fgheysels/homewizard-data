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

        public ElectricityController(InfluxDbReader influxDbReader)
        {
            _influxInfluxDbReader = influxDbReader;
        }

        [HttpGet("daily")]
        public async Task<IActionResult> GetDailyElectricityData(DateTimeOffset fromDate, DateTimeOffset toDate)
        {
            string influxQuery =
                "SELECT * FROM electricity ";

            var results = await _influxInfluxDbReader.QueryQLAsync(influxQuery);
            return Ok(results);
        }
    }
}
