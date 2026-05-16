using Microsoft.AspNetCore.Mvc;

namespace MathApi_Client.Controllers
{
    public class HealthController : Controller
    {
        [HttpGet("/health")]
        public IActionResult Index()
        {
            return Content("ok");
        }
    }
}