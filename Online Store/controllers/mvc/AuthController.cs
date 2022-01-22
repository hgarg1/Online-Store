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

            StringValues successCallback = new StringValues();
            if(Request.Query.TryGetValue("success", out successCallback))
            {

                if (String.Equals(successCallback, "false"))
                {
                    StringValues messageCallback = new StringValues();
                    if (Request.Query.TryGetValue("message", out messageCallback))
                    {
                        if (messageCallback.Equals("email"))
                        {
                            Login model = new Login("Email Verification Sent! Please verify and try logging in again. Need to change your email?");
                            return View(model);
                        }
                    }
                    else
                    {
                        Login model = new Login("User name or password didn't work, please try again.");
                        return View(model);
                    }
                }
                else if (String.Equals(successCallback, "true"))
                {
                    StringValues messageCallback = new StringValues();
                    if (Request.Query.TryGetValue("message", out messageCallback))
                    {
                        if (messageCallback.Equals("reset"))
                        {
                            Login model = new Login("Your Password Has Been Successfully Reset Please Login Now! Thanks for shopping with us!");
                            return View(model);
                        }
                    }
                }
            }
            AuthFilter auth = new AuthFilter(_configuration);
            if (auth.isValid(HttpContext.Session.GetString("user")))
            {
                User user = JsonSerializer.Deserialize<User>(HttpContext.Session.GetString("user"));
                return RedirectToAction("GetLogin", "Auth", new UserLogin() { username = user.email, password = user.password });
            }
            return View(new Login(""));
        }

        [HttpGet("/[action]")]
        public IActionResult Signup()
        {
            StringValues successCallback = new StringValues();
            if (Request.Query.TryGetValue("success", out successCallback))
            {
                Console.WriteLine("success val: " +  successCallback);
                if (String.Equals(successCallback, "false"))
                {
                    Console.WriteLine("something bad happened");
                    StringValues messageCallbback = new StringValues();
                    if (Request.Query.TryGetValue("message", out messageCallbback))
                    {
                        if(String.Equals(messageCallbback, "0"))
                        {
                            Console.WriteLine("first error message case!");
                            Signup model = new Signup() { error = true, Message = "Please make sure passwords match!"};
                            return View(model);
                        }
                        else if (String.Equals(messageCallbback, "1"))
                        {
                            Signup model = new Signup() { error = true, Message = "User Already Exists!" };
                            return View(model);
                        }
                    }
                }
                else if(String.Equals(successCallback, "true"))
                {
                    Signup model = new Signup() { error = false, Message = "Success! Account Created! Please verify your email by clicking the link sent to it before you login (wrong email? click the button below to change & resend)." };
                    return View(model);
                }
            }
            return View(new Signup());
        }

        [HttpGet("/[action]")]
        public IActionResult Email()
        {
            StringValues successCallback = new StringValues();
            if (Request.Query.TryGetValue("success", out successCallback))
            {
                Console.WriteLine("success val: " + successCallback);
                if (String.Equals(successCallback, "false"))
                {
                    Console.WriteLine("something bad happened");
                    StringValues messageCallbback = new StringValues();
                    if (Request.Query.TryGetValue("message", out messageCallbback))
                    {
                        if (String.Equals(messageCallbback, "0"))
                        {
                            Email toInsert = new Email() { IsActive = false, Title = "Username not found please try again please by attempting to login", Message = "Welcome to the Email Verification System. Please input your email and click the link that is sent. Thank you for shopping with us!" };
                            return View(toInsert);
                        }
                        else if (String.Equals(messageCallbback, "1"))
                        {
                            Email toInsert = new Email() { IsActive = false, Title="Invalid Token Please request a new one.", Message = "Welcome to the Email Verification System. Please input your email and click the link that is sent. Thank you for shopping with us!" };
                            return View(toInsert);
                        }
                        else if (String.Equals(messageCallbback, "2"))
                        {
                            Email toInsert = new Email() { IsActive = false, Title="Your token is either expired or was never generated, please try again.", Message = "Welcome to the Email Verification System. Please input your email and click the link that is sent. Thank you for shopping with us!" };
                            return View(toInsert);
                        }
                        else if (String.Equals(messageCallbback, "3"))
                        {
                            Email toInsert = new Email() { IsActive = false, Title = "Please fill out the form to request a new link to the email", Message = "Welcome to the Email Verification System. Please input your email and click the link that is sent. Thank you for shopping with us!" };
                            return View(toInsert);
                        }
                        else if (String.Equals(messageCallbback, "4"))
                        {
                            Email toInsert = new Email() { IsActive = false, Title = "That Email Already In Use Please Reset Password", Message = "Your email is already being used for another account. Please try another email or use the forget password feature." };
                            return View(toInsert);
                        }
                    }
                }
                else if (String.Equals(successCallback, "true"))
                {
                    Email toInsert = new Email() { IsActive = true, Message = "Congratulations you are all set! You may no procede to login by clicking the button below", Title = "Email Verification Tool" };
                    return View(toInsert);
                }
            }
            Email model = new Email() { IsActive = false, Title = "Please fill out the form to request a new link to the email", Message = "Welcome to the Email Verification System. Please input your email and click the link that is sent. Thank you for shopping with us!" };
            return View(model);
        }

        [HttpGet("/[action]")]
        public IActionResult ForgotPassword()
        {
            StringValues successCallback = new StringValues();
            if (Request.Query.TryGetValue("success", out successCallback))
            {
                Console.WriteLine("success val: " + successCallback);
                if (String.Equals(successCallback, "false"))
                {
                    Console.WriteLine("something bad happened");
                    StringValues messageCallback = new StringValues();
                    if (Request.Query.TryGetValue("message", out messageCallback))
                    {
                        if (String.Equals(messageCallback, "user"))
                        {
                            ForgotPassword model = new ForgotPassword() { message = "Email Provided is invalid.", showSetPassword = false };
                            return View(model);
                        }else if (String.Equals(messageCallback, "token")){
                            ForgotPassword model = new ForgotPassword() { message = "Token Provided is invalid. Please try again using the form.", showSetPassword = false };
                            return View(model);
                        }
                        else if (String.Equals(messageCallback, "session"))
                        {
                            ForgotPassword model = new ForgotPassword() { message = "Session Expired Please Request A New Link", showSetPassword = false };
                            return View(model);
                        }
                    }
                }else if (String.Equals(successCallback, "true"))
                {
                    StringValues messageCallbback = new StringValues();
                    if (Request.Query.TryGetValue("message", out messageCallbback))
                    {
                        if (String.Equals(messageCallbback, "email"))
                        {
                            ForgotPassword model = new ForgotPassword() { message = "Sucess! Email is sent please paste the link in THIS browser.", showSetPassword = false };
                            return View(model);
                        }else if(String.Equals(messageCallbback, "accepted"))
                        {
                            ForgotPassword model = new ForgotPassword() { message = "Sucess! Token accepted please enter user details", showSetPassword = true };
                            return View(model);
                        }
                    }
                        
                }
            }
            ForgotPassword pswdModel = new ForgotPassword() { message = "Forgot Password to Your Online Store? No worries, enter the email and we will send a link where you can set a new one", showSetPassword = false };
            return View(pswdModel);
        }
    }
}
