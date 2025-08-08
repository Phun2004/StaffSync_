using Microsoft.AspNetCore.Mvc;

namespace Demo.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
