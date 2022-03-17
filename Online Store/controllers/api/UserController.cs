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
        private AuthController authNeeds;
        public UserController(IConfiguration configuration)
        {
            _configuration = configuration;
            authNeeds = new AuthController(_configuration); //pass through auth
        }

        [HttpPost("[action]")]
        public void Update([FromForm]User req)
        {
            SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("SQL"));
            sqlConnection.Open();

            req.emailOld = JsonSerializer.Deserialize<Models.User>(HttpContext.Session.GetString("user")).email;
            sqlConnection.Close();
            HttpContext.Session.SetString("user", JsonSerializer.Serialize(req)); //updates cache
            if(!String.Equals(req.email, req.emailOld))
            {
                sqlConnection.Execute("update [user] set firstName = @firstName, lastName = @lastName, email = @email, password = @password, age=@age, sex=@sex, address = @address, emailVerified = 'false', ethnicity = @ethnicity where email = @emailOld", req);
                authNeeds.SendEmailValidation(req.email, true);
                authNeeds.Logout(true, false);
                Response.Redirect("/Login?success=false&message=email");
                return;
            }
            else
            {
                Console.WriteLine("same email being updated!");
                sqlConnection.Execute("update [user] set firstName = @firstName, lastName = @lastName, email = @email, password = @password, age=@age, sex=@sex, address = @address, emailVerified = 'true', ethnicity = @ethnicity where email = @emailOld", req);
                Response.Redirect("/Settings");
                return;
            }
        }

        [HttpPost("[action]")]
        public String Delete()
        {
            SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("SQL"));
            sqlConnection.Open();

            Models.User req = JsonSerializer.Deserialize<Models.User>(HttpContext.Session.GetString("user"));

            sqlConnection.Execute("delete from [user] where email = @email AND password = @password", new { 
                @email = req.email, @password = req.password 
            });
            sqlConnection.Close();

            HttpContext.Session.Clear();
            return "/Login";    
        }
    }
}
