using Microsoft.AspNetCore.Mvc;

namespace ERP.Controllers
{
    public class HealthController : Controller
    {
        [HttpGet]
        public IActionResult Health()
        {
            return Ok("Healthy");
        }
    }
}
