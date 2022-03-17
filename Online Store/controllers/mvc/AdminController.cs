using Microsoft.AspNetCore.Mvc;

namespace Online_Store.controllers.mvc
{
    [Route("/[controller]")]
    public class AdminController : Controller
    {
        [HttpGet("[action]")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("[action]")]
        public IActionResult Settings()
        {
            return View();
        }
    }
}
