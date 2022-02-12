using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Models;
using Dapper;
using System.Text.Json;

namespace Online_Store.controllers.api
{
    [Route("/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private IConfiguration _configuration;
        public UserController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("[action]")]
        public void Update([FromForm]User req)
        {
            SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("SQL"));
            sqlConnection.Open();

            req.emailOld = JsonSerializer.Deserialize<Models.User>(HttpContext.Session.GetString("user")).email;
            sqlConnection.Close();
            HttpContext.Session.SetString("user", JsonSerializer.Serialize(req)); //updates cache
            AuthController authNeeds = new AuthController(_configuration); //pass through auth like last time
            if(!String.Equals(req.email, req.email))
            {
                sqlConnection.Execute("update [user] set firstName = @firstName, lastName = @lastName, email = @email, password = @password, age=@age, sex=@sex, address = @address, emailVerified = 'false', ethnicity = @ethnicity where email = @emailOld", req);
                authNeeds.SendEmailValidation(req.email, true, HttpContext, true);
                authNeeds.Logout(true, false);
                Response.Redirect("/Login?success=false&message=email");
                return;
            }
            else
            {
                sqlConnection.Execute("update [user] set firstName = @firstName, lastName = @lastName, email = @email, password = @password, age=@age, sex=@sex, address = @address, emailVerified = 'true', ethnicity = @ethnicity where email = @emailOld", req);
                authNeeds.Logout(true, false);
                Response.Redirect("/Login");
                return;
            }
        }
    }
}
