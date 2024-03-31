using Microsoft.AspNetCore.Mvc;

namespace Fg.Homewizard.EnergyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : Controller
    {
        [HttpGet]
        public IActionResult Health()
        {
            return Ok("healthy");
        }
    }
}
