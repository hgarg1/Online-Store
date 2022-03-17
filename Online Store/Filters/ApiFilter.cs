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
        // Do something before the action executes.
    }

    public void OnActionExecuted(ActionExecutedContext context){}
}