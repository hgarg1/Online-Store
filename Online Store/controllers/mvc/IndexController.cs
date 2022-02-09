using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Online_Store.pagemodels;
using System.Text.Json;

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
            StringValues successCallback = new StringValues();
            if (Request.Query.TryGetValue("success", out successCallback))
            {
                if (String.Equals(successCallback, "false"))
                {
                    StringValues messageCallbback = new StringValues();
                    if (Request.Query.TryGetValue("message", out messageCallbback))
                    {
                        if (String.Equals(messageCallbback, "0"))
                        {
                            pagemodels.Index model = new pagemodels.Index() { error = true, Message = "User Session Expired, please close and reopen tab" };
                            return View(model);
                        }
                    }
                }
                else if (String.Equals(successCallback, "true"))
                {
                    pagemodels.Index model = new pagemodels.Index() { error = false, Message = "" };
                    return View(model);
                }
            }
            return View(new pagemodels.Index() { error = false, Message = "" });
        }

        [HttpGet("/[action]")]
        public IActionResult About()
        {
            return View();
        }

        [HttpGet("/[action]")]
        public IActionResult Contact()
        {
            StringValues successCallback = new StringValues();
            if (Request.Query.TryGetValue("success", out successCallback))
            {

                if (String.Equals(successCallback, "false"))
                {
                    return View(new Contact { IsSuccess = false, Message = "Something Wrong Happened Please Try Again!"});
                }
                else if(String.Equals(successCallback, "true"))
                {
                    return View(new Contact {  Message = "Success! Email sent to recipient." });
                }
            }
            return View(new Contact { IsSuccess = null, Message = "" });
        }

        [HttpGet("[action]")]
        public IActionResult Settings()
        {
            Settings model = JsonSerializer.Deserialize<Settings>(HttpContext.Session.GetString("user"));
            model.IsActive = true;
            return View(model);
        }
    }
}
