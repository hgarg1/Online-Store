using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Binders;
using Online_Store.Filters;
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
        public AuthController(IConfiguration configuration) //allows us to get connection string
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
                Models.User user = users.First();
                if (user.Equals(req))
                {
                    req.lastLogin = DateTime.Now.ToString();
                    sqlConnection.Execute("update [user] set lastLogin = @lastLogin where email = @username", req);
                    sqlConnection.Close();
                    HttpContext.Session.SetString("user", JsonSerializer.Serialize(user));//set session object for authentication into restircted pages
                    
                    Response.Redirect("/Index");
                }
                else
                {
                    sqlConnection.Close();
                    Response.Redirect("/Login?success=false");//add error so login can display such message
                }
            }
        }

        [HttpGet("[action]")]
        public void GetLogin() //same as above but GET
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
                Models.User user = users.First();
                if (user.Equals(req))
                {
                    req.lastLogin = DateTime.Now.ToString();
                    sqlConnection.Execute("update [user] set lastLogin = @lastLogin where email = @username", req); //updates last login
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

            AuthFilter auth = new AuthFilter(_configuration);//pass through auth, providing means without adittional calls
            if (!auth.isValid(HttpContext.Session.GetString("user")))
            {
                Response.Redirect("/Index?success=false&message=0");
                return;
            }
            HttpContext.Session.Clear();
            Response.Redirect("/Login");
        }

        [HttpPost("[action]")]
        public void Signup([FromForm] Binders.UserSignup req)
        {
            Console.WriteLine(req.password + " " + req.confPassword + " " + String.Equals(req.password, req.confPassword));
            if(!(String.Equals(req.password, req.confPassword)))
            {
                Response.Redirect("/Signup?success=false&message=0");
                return;
            }
            SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("SQL"));
            sqlConnection.Open();
            Models.User? isExisting = sqlConnection.Query<Models.User>("select * from [user] where email = @email", req).SingleOrDefault(); 
            if(isExisting == null)
            {
                req.lastLogin = null;
                int rowsAffected = sqlConnection.Execute("insert into [user] (firstName, lastName, email, lastLogin, password, address, sex, age) " +
                                                                    "values (@firstName, @lastName, @email, @lastLogin, @password, @address, @sex, @age)", req); //inserts bound object into data
                //statement above is sysnonymous with a prepared statement
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
