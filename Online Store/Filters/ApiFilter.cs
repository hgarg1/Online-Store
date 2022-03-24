using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Dapper;
using Models;
using Microsoft.Data.SqlClient;
using System.Text.Json;
public class ApiFilter : IActionFilter
{
    IConfiguration _configuration;
    public ApiFilter(IConfiguration config)
    {
        _configuration = config;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if(context.HttpContext.Session.GetString("user") == null)
        {
            return;
        }

        User user = JsonSerializer.Deserialize<User>(context.HttpContext.Session.GetString("user"));
        if (user == null)
        {
            context.Result = new RedirectResult("/Login");
        }

        string reqApiUrl = context.HttpContext.Request.Path;
        if(reqApiUrl.IndexOf("Admin") != -1)
        {
            if (user.role.Equals("User"))
            {
                context.Result = new RedirectResult("/Login");
            }
        }
    }

    public void OnActionExecuted(ActionExecutedContext context){}
}