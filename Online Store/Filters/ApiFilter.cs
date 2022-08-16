using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Dapper;
using Models;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using System.Diagnostics;

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
        User? user = JsonSerializer.Deserialize<User>(context.HttpContext.Session.GetString("user"),new JsonSerializerOptions()
        {
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
        });
        if (user == null)
        {
            context.Result = new RedirectResult("/Login");
        }

        string reqApiUrl = context.HttpContext.Request.Path;
        Console.WriteLine(reqApiUrl);
        if (reqApiUrl.IndexOf("IsAdmin")!=-1)//whitelist
        {
            return;
        }

        if(reqApiUrl.IndexOf("Admin") != -1)
        {
            OnlineStore ctx = new OnlineStore();
            if (user.Role == ctx.Roles.Where(r => r.RoleName.Equals("User")).First().Id)
            {
                context.Result = new RedirectResult("/Login");
            }
        }
    }

    public void OnActionExecuted(ActionExecutedContext context){}
}