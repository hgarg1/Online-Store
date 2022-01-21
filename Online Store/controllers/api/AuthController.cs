using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Binders;
using Online_Store.Filters;
using Dapper;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Google.Apis.Gmail.v1;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Online_Store.controllers.api
{

    [Route("/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private static Dictionary<string, string[]> userTokens = new Dictionary<string, string[]>();

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
                if (user.Equals(req) && user.emailVerified.Equals("true"))
                {
                    req.lastLogin = DateTime.Now.ToString();
                    sqlConnection.Execute("update [user] set lastLogin = @lastLogin where email = @username", req);
                    sqlConnection.Close();
                    HttpContext.Session.SetString("user", JsonSerializer.Serialize(user));//set session object for authentication into restircted pages
                    
                    Response.Redirect("/Index");
                }
                else if (user.Equals(req) && user.emailVerified.Equals("false"))
                {
                    sqlConnection.Close();
                    HttpContext.Session.SetString("user", JsonSerializer.Serialize(user));
                    SendEmailValidation(req.username, true);
                    Response.Redirect("/Login?success=false&message=email");
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
                Console.WriteLine("email status = " + user.emailVerified);
                if (user.Equals(req) && user.emailVerified.Equals("true"))
                {
                    req.lastLogin = DateTime.Now.ToString();
                    sqlConnection.Execute("update [user] set lastLogin = @lastLogin where email = @username", req); //updates last login
                    sqlConnection.Close();
                    HttpContext.Session.SetString("user", JsonSerializer.Serialize(user));

                    Response.Redirect("/Index");
                }
                else if (user.Equals(req) && user.emailVerified.Equals("false"))
                {
                    sqlConnection.Close();
                    HttpContext.Session.SetString("user", JsonSerializer.Serialize(user));
                    SendEmailValidation(req.username, true);
                    Response.Redirect("/Login?success=false&message=email");
                }
                else
                {
                    sqlConnection.Close();
                    Response.Redirect("/Login?success=false");
                }
            }
        }

        [HttpPost("[action]")]
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
                int rowsAffected = sqlConnection.Execute("insert into [user] (firstName, lastName, email, lastLogin, password, address, sex, age, emailVerified) " +
                                                                    "values (@firstName, @lastName, @email, @lastLogin, @password, @address, @sex, @age, 'false')", req); //inserts bound object into data
                //statement above is sysnonymous to a prepared statement
                sqlConnection.Close();
                //partial cache save, only enough for send email to work
                HttpContext.Session.SetString("user", JsonSerializer.Serialize(new Models.User() { firstName = req.firstName, lastName = req.lastName}));
                SendEmailValidation(req.email, true);
                Response.Redirect("/Signup?success=true");
            }
            else
            {
                sqlConnection.Close();
                Response.Redirect("/Signup?success=false&message=1");
            }
        }

        [HttpGet("[action]/{token}")]
        public void ValidateEmail(string token)
        {
            string? cache = HttpContext.Session.GetString("user");
            if(cache == null)
            {
                Response.Redirect("/Email?success=false&message=0"); //user doesn't exist
                return;
            }

            Models.User reqUser = JsonSerializer.Deserialize<Models.User>(HttpContext.Session.GetString("user"));
            string?[] arrayVal = { };
            Console.WriteLine("key:" + reqUser.firstName + " " + reqUser.lastName);
            if (userTokens.TryGetValue(reqUser.firstName + " " + reqUser.lastName, out arrayVal))//handles if user requests new link, voids old one
            {
                Console.WriteLine("Token Provided: " + token + " Token Found in DB: " + arrayVal[0]);
                if (!arrayVal[0].Equals(token))
                {
                    Response.Redirect("/Email?success=false&message=1"); //wrong token
                    return;
                }
                arrayVal[0] = null;
                userTokens.Remove(reqUser.firstName + " " + reqUser.lastName);

                SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("SQL"));
                sqlConnection.Execute("update [user] set emailVerified = 'true' where email = @email", new {@email = reqUser.email});
                sqlConnection.Execute("update [user] set email = @emailNew where email = @emailOld", new { @emailNew= arrayVal[1], @emailOld = reqUser.email });
                sqlConnection.Close();

                Response.Redirect("/Email?success=true");
            }
            else
            {
                Response.Redirect("/Email?success=false&message=2"); //expired or non existent link
            }
        }
        
        [HttpGet("[action]")]
        public void SendEmailValidation([FromQuery] string email, bool noRedirect = false)
        {
            Console.WriteLine(email);

            string? cache = HttpContext.Session.GetString("user");
            if (cache == null)
            {
                Response.Redirect("/Email?success=false&message=0"); //user doesn't exist
                return;
            }
            Models.User reqUser = JsonSerializer.Deserialize<Models.User>(HttpContext.Session.GetString("user"));

            Models.User user = new SqlConnection(_configuration.GetConnectionString("SQL")).Query<Models.User>("select * from [user] where firstName = @firstName AND lastName = @lastName", 
                new {@firstName = reqUser.firstName,
                     @lastName = reqUser.lastName
            }).First();
            if(user == null)
            {
                Response.Redirect("/Email?success=false&message=0"); //user doesn't exist
                return;
            }

            String randomKey = GetUniqueKey(32);
            string[]? arrayVal = { };
            if (userTokens.TryGetValue(reqUser.firstName + " " + reqUser.lastName, out arrayVal))//handles if user requests new link, voids old one
            {
                
                arrayVal[0] = null;
                userTokens.Remove(reqUser.firstName + " " + reqUser.lastName);
                userTokens.Add(reqUser.firstName + " " + reqUser.lastName, new string[] { randomKey, email});
            }
            else
            {
                userTokens.Add(reqUser.firstName + " " + reqUser.lastName, new string[] { randomKey, email });
            }

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential("hgarg1@terpmail.umd.edu", "Deepak@2003_101");
            smtpClient.Send(new MailMessage("hgarg1@terpmail.umd.edu", email, "Your Access Link | Online Store", $"Please use this url in the SAME browser when confirming your email.\n Link: https://localhost:7108/Auth/ValidateEmail/{randomKey}\nThe link will expire in 5 hours.\nThanks for shopping with us!\nSincerely, Online Store Team"));
            Task.Delay(new TimeSpan(5, 0, 0)).ContinueWith(action => { 
                userTokens.Remove(reqUser.firstName + " " + reqUser.lastName);
            });
            if (noRedirect)
            {
                return;
            }
            else
            {
                Response.Redirect("/Email?success=true");
            }
        }

        public string GetUniqueKey(int size)
        {
            char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

            byte[] data = new byte[4 * size];
            using (var crypto = RandomNumberGenerator.Create())
            {
                crypto.GetBytes(data);
            }
            StringBuilder result = new StringBuilder(size);
            for (int i = 0; i < size; i++)
            {
                var rnd = BitConverter.ToUInt32(data, i * 4);
                var idx = rnd % chars.Length;

                result.Append(chars[idx]);
            }
            return result.ToString();
        }
    }
}
