using Fg.Homewizard.EnergyApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fg.Homewizard.EnergyApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElectricityController : ControllerBase
    {
        private readonly EnergyConsumptionRetriever _energyService;
        private readonly ILogger<ElectricityController> _logger;

        public ElectricityController(EnergyConsumptionRetriever energyService, ILogger<ElectricityController> logger)
        {
            _energyService = energyService;
            _logger = logger;
        }

        [HttpGet("daily")]
        [Produces( "application/json", "text/csv")]
        public async Task<IActionResult> GetDailyElectricityData(DateTimeOffset fromDate, DateTimeOffset toDate)
        {
            var result = await _energyService.GetElectricityConsumptionForPeriodAsync(fromDate, toDate);

            return Ok(result);
        }
    }
}
