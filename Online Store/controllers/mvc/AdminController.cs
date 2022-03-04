using Microsoft.AspNetCore.Mvc;

namespace Online_Store.controllers.mvc
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
