using Binders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Models;
using Online_Store.Filters;
using Online_Store.pagemodels;
using System.Text.Json;

namespace Online_Store.controllers.mvc
{
    public class AuthController : Controller
    {

        private IConfiguration _configuration;
        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpGet("/[action]")]
        public IActionResult Login()
        {

            AuthFilter auth = new AuthFilter(_configuration);
            if (auth.isValid(HttpContext.Session.GetString("user")))
            {
                User user = JsonSerializer.Deserialize<User>(HttpContext.Session.GetString("user"));
                return RedirectToAction("GetLogin", "Auth", new UserLogin() {username = user.email, password = user.password});
            }
            StringValues successCallbback = new StringValues();
            if(Request.Query.TryGetValue("success", out successCallbback))
            {
                if(String.Equals(successCallbback, "false"))
                {
                    Login model = new Login("User name or password didn't work, please try again.");
                    return View(model);
                }
            }
            return View(new Login(""));
        }

        [HttpGet("/[action]")]
        public IActionResult Signup()
        {
            StringValues successCallbback = new StringValues();
            if (Request.Query.TryGetValue("success", out successCallbback))
            {

                if (String.Equals(successCallbback, "false"))
                {
                    StringValues messageCallbback = new StringValues();
                    if (Request.Query.TryGetValue("message", out messageCallbback))
                    {
                        if(String.Equals(messageCallbback, "0"))
                        {
                            Signup model = new Signup() { error = true, Message = "Please make sure passwords match!"};
                            return View(model);
                        }
                        else if (String.Equals(messageCallbback, "1"))
                        {
                            Signup model = new Signup() { error = true, Message = "user Already Exists!" };
                            return View(model);
                        }
                    }
                }
                else if(String.Equals(successCallbback, "true"))
                {
                    Signup model = new Signup() { error = false, Message = "Success! Account Created!" };
                    return View(model);
                }
            }
            return View(new Signup());
        }
    }
}
