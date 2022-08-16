using Microsoft.AspNetCore.Mvc;
using Models;
using Online_Store.pagemodels;
using System.Text.Json;

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
            return View(JsonSerializer.Deserialize<Settings>(HttpContext.Session.GetString("user"), new JsonSerializerOptions()
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
            }));
        }

        [HttpGet("[action]")]
        public IActionResult UserManagement()
        {
            return View();
        }

        [HttpGet("[action]")]
        public IActionResult ItemManagement()
        {
            return View();
        }
    }
}
