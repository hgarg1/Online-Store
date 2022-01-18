using Microsoft.AspNetCore.Mvc;

namespace Online_Store.controllers.mvc
{
    public class IndexController : Controller
    {
        [HttpGet("/")]
        public void Root()
        {
            Response.Redirect("/Login");
        }

        [HttpGet("/[action]")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("/[action]")]
        public IActionResult About()
        {
            return View();
        }

        [HttpGet("/[action]")]
        public IActionResult ContactUs()
        {
            return View("~/views/index/contact.cshtml");
        }
    }
}
