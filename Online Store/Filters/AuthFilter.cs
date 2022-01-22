using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Dapper;
using Models;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace Online_Store.Filters
{
    public class AuthFilter : IActionFilter
    {
        IConfiguration _configuration;
        public AuthFilter(IConfiguration config)
        {
            _configuration = config;
        }

        //before it reaches MVC controller
        public void OnActionExecuting(ActionExecutingContext context)
        {
            string? user = context.HttpContext.Session.GetString("user");
            //first part is list of whitelisted pages, do NOT black list but rather whitelist
            if (
                (
                context.HttpContext.Request.Path.ToUriComponent().IndexOf("/Login") == -1 
                && context.HttpContext.Request.Path.ToUriComponent().IndexOf("/Signup") == -1
                && context.HttpContext.Request.Path.ToUriComponent().IndexOf("/Email") == -1
                && context.HttpContext.Request.Path.ToUriComponent().IndexOf("/Auth/ValidateEmail") == -1
                && context.HttpContext.Request.Path.ToUriComponent().IndexOf("/Auth/SendEmailValidation") == -1
                && context.HttpContext.Request.Path.ToUriComponent().IndexOf("/Auth/ValidatePasswordResetLink") == -1
                && context.HttpContext.Request.Path.ToUriComponent().IndexOf("/Auth/SetPassword") == -1
                && context.HttpContext.Request.Path.ToUriComponent().IndexOf("/ForgotPassword") == -1
                ) 
                && 
                (
                user == null 
                || !isValid(user)
                )
            )
            {
                //context.Result = new FileStreamResult(fileStream: new FileStream("wwwroot/403.html", FileMode.Open), contentType: "text/html");
                context.Result = new RedirectResult("/Login");
            }
        }

        public bool isValid(string user)//used a lot, self explanatory
        {
            if(user == null || String.Equals(user, "")) { return false; } //precheck

            SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("SQL"));
            sqlConnection.Open();
            User? _user = JsonSerializer.Deserialize<User>(user);

            if(_user == null ) { return false; }


            IEnumerable<User> users = sqlConnection.Query<User>("select * from [user] where email = @email and password = @password", _user);
            sqlConnection.Close();
            Console.WriteLine(users);
            if(users.Count() == 0) 
            { 
                return false;
            }
            else
            {
                User? foundUser = users.FirstOrDefault();
                Console.WriteLine(foundUser);
                Console.WriteLine(_user);
                if (foundUser == null) { return false;}


                if (foundUser.Equals(_user))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        //after it reaches MVC controller
        public void OnActionExecuted(ActionExecutedContext context){}//most probably never used as filter wouldn't be a filter then
    }
}
