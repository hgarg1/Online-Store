using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Binders;
using Dapper;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace Online_Store.controllers.api
{
    [Route("/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private IConfiguration _configuration;
        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("[action]")]
        public void Login([FromForm] Binders.UserLogin req)
        {
            Console.WriteLine("incoming user: " + req.username + " " + req.password);
            SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("SQL"));
            sqlConnection.Open();
            IEnumerable<Models.User> users = sqlConnection.Query<Models.User>("select * from [user] where email = @username AND password = @password", req);
            if(users.Count() == 0 || users.Count() > 1)
            {
                Response.Redirect("/Login?success=false");
            }
            else
            {
                Console.WriteLine("Found record!--");
                Models.User user = users.First();
                if (user.Equals(req))
                {
                    req.lastLogin = DateTime.Now.ToString();
                    sqlConnection.Execute("update [user] set lastLogin = @lastLogin where email = @username", req);
                    sqlConnection.Close();
                    HttpContext.Session.SetString("user", JsonSerializer.Serialize(user));
                    
                    Response.Redirect("/Index");
                }
                else
                {
                    sqlConnection.Close();
                    Response.Redirect("/Login?success=false");
                }
            }
        }

        [HttpGet("[action]")]
        public void GetLogin()
        {

            Binders.UserLogin req = new Binders.UserLogin() { username = HttpContext.Request.Query["username"], password = HttpContext.Request.Query["password"] };
            Console.WriteLine("incoming user: " + req.username + " " + req.password);
            SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("SQL"));
            sqlConnection.Open();
            IEnumerable<Models.User> users = sqlConnection.Query<Models.User>("select * from [user] where email = @username AND password = @password", req);
            if (users.Count() == 0 || users.Count() > 1)
            {
                Response.Redirect("/Login?success=false");
            }
            else
            {
                Console.WriteLine("Found record!--");
                Models.User user = users.First();
                if (user.Equals(req))
                {
                    req.lastLogin = DateTime.Now.ToString();
                    sqlConnection.Execute("update [user] set lastLogin = @lastLogin where email = @username", req);
                    sqlConnection.Close();
                    HttpContext.Session.SetString("user", JsonSerializer.Serialize(user));

                    Response.Redirect("/Index");
                }
                else
                {
                    sqlConnection.Close();
                    Response.Redirect("/Login?success=false");
                }
            }
        }

        [HttpGet("[action]")]
        public void Logout()
        {
            HttpContext.Session.Clear();
            Response.Redirect("/Login");
        }

        [HttpPost("[action]")]
        public void Signup([FromForm] Binders.UserSignup req)
        {
            if(!String.Equals(req.password, req.confPassword))
            {
                Response.Redirect("/Signup?success=false&message=0");
            }
            SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("SQL"));
            sqlConnection.Open();
            Models.User? isExisting = sqlConnection.Query<Models.User>("select * from [user] where email = @email", req).SingleOrDefault(); 
            if(isExisting == null)
            {
                req.lastLogin = null;
                int rowsAffected = sqlConnection.Execute("insert into [user] (firstName, lastName, email, lastLogin, password, address, sex, age) " +
                    "                                               values (@firstName, @lastName, @email, @lastLogin, @password, @address, @sex, @age)", req);
                sqlConnection.Close();
                Response.Redirect("/Signup?success=true");
            }
            else
            {
                sqlConnection.Close();
                Response.Redirect("/Signup?success=false&message=1");
            }
        }
    }
}
