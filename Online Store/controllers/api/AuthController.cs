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
                    user.lastLogin = req.lastLogin;
                    HttpContext.Session.SetString("user", JsonSerializer.Serialize(user));//set session object for authentication into restircted pages
                    Response.Redirect("/Index");
                }
                else if (user.Equals(req) && user.emailVerified.Equals("false"))
                {
                    sqlConnection.Close();
                    HttpContext.Session.SetString("user", JsonSerializer.Serialize(user));
                    SendEmailValidation(req.username, false, null, true);
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
                    user.lastLogin = req.lastLogin;
                    HttpContext.Session.SetString("user", JsonSerializer.Serialize(user));
                    SendEmailValidation(req.username, false, null, true);
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
        public void Logout(bool bypassCacheCheck = false, bool redirect = true)
        {

            AuthFilter auth = new AuthFilter(_configuration);//pass through auth, providing means without adittional calls
            if (!bypassCacheCheck && !auth.isValid(HttpContext.Session.GetString("user")))
            {
                Response.Redirect("/Index?success=false&message=0");
                return;
            }
            HttpContext.Session.Clear();
            if (redirect)
            {
                Response.Redirect("/Login");
            }
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
                SendEmailValidation(req.email, false, null, true);
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
                reqUser.emailVerified = "true";
                reqUser.email = arrayVal[0];
                HttpContext.Session.SetString("user", JsonSerializer.Serialize(reqUser)); //updates cache


                Response.Redirect("/Email?success=true");
            }
            else
            {
                Response.Redirect("/Email?success=false&message=2"); //expired or non existent link
            }
        }
        
        [HttpGet("[action]")]
        public void SendEmailValidation([FromQuery] string email, bool bypassCacheCheck = false, string? userCache = null, bool noRedirect = false)
        {
            Console.WriteLine(email);

            string? cache = null;
            if (bypassCacheCheck)
            {
                cache = userCache;
            }
            else
            {
                cache = HttpContext.Session.GetString("user");
                if (cache == null)
                {
                    Response.Redirect("/Email?success=false&message=0"); //user doesn't exist
                    return;
                }
            }
            

            Models.User reqUser = JsonSerializer.Deserialize<Models.User>(cache);

            Models.User user = new SqlConnection(_configuration.GetConnectionString("SQL")).Query<Models.User>("select * from [user] where email = @email", 
                new {@email = email}
            ).First();

            if(user != null)
            {
                Response.Redirect("/Email?success=false&message=4"); //email already mapped to someone
                return;
            }

            if (user.emailVerified.Equals("true"))
            {
                Response.Redirect("/");
                return;
            }

            String randomKey = GetUniqueKey(32);
            string[]? arrayVal = { };
            if (userTokens.TryGetValue(reqUser.firstName + " " + reqUser.lastName, out arrayVal))//handles if user requests new link, voids old one
            {
                
                arrayVal[0] = null;
                userTokens.Remove(reqUser.firstName + " " + reqUser.lastName);
                userTokens.Add(reqUser.firstName + " " + reqUser.lastName, new string[] { randomKey, email});
                //updates uer db and cache using prior token email pair to as old email
                new SqlConnection(_configuration.GetConnectionString("SQL")).Execute("update [user] set email = @emailNew where email = @emailOld", new { @emailNew = email, @emailOld = arrayVal[1] });
                reqUser.email = email;
                if (!bypassCacheCheck)
                {
                    HttpContext.Session.SetString("user", JsonSerializer.Serialize(reqUser));
                }
                //
            }
            else
            {
                userTokens.Add(reqUser.firstName + " " + reqUser.lastName, new string[] { randomKey, email });
                //updates uer db and cache using cache as old email
                new SqlConnection(_configuration.GetConnectionString("SQL")).Execute("update [user] set email = @emailNew where email = @emailOld", new { @emailNew = email, @emailOld = reqUser.email });
                reqUser.email = email;
                if (!bypassCacheCheck)
                {
                    HttpContext.Session.SetString("user", JsonSerializer.Serialize(reqUser));
                }
                //
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
                Response.Redirect("/Login?success=false&message=email");
            }
        }

        private string GetUniqueKey(int size)
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

        [HttpPost("[action]")]
        public void ForgotPassword([FromForm] Binders.UserLogin req)
        {
            SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("SQL"));
            sqlConnection.Open();
            Models.User user = sqlConnection.Query<Models.User>("select * from [user] where email = @email", new {@email = req.username}).First();
            if (user == null)
            {
                Response.Redirect("/ForgotPassword?success=false&message=user");
                return;
            }
            sqlConnection.Close();

            string[] arrayVal = null;
            if (userTokens.TryGetValue(user.firstName + " " + user.lastName, out arrayVal))
            {
                userTokens.Remove(user.firstName + " " + user.lastName);
                arrayVal = new string[]{ GetUniqueKey(32), req.username};
                userTokens.Add(user.firstName + " " + user.lastName, arrayVal);
            }
            else
            {
                arrayVal = new string[]{ GetUniqueKey(32), req.username};
                userTokens.Add(user.firstName + " " + user.lastName, arrayVal);
            }

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential("hgarg1@terpmail.umd.edu", "Deepak@2003_101");
            smtpClient.Send(new MailMessage("hgarg1@terpmail.umd.edu", req.username, "Your Password Reset Link Link | Online Store", $"Please use this url in the SAME browser when resseting your password.\nLink: https://localhost:7108/Auth/ValidatePasswordResetLink/{arrayVal[0]}\nThe link will expire in 5 hours.\nThanks for shopping with us!\nSincerely, Online Store Team"));

            HttpContext.Session.SetString("user", req.username);

            Task.Delay(new TimeSpan(5, 0, 0)).ContinueWith(action => {
                userTokens.Remove(user.firstName + " " + user.lastName);
            });
            Response.Redirect("/ForgotPassword?success=true&message=email");
        }

        [HttpGet("[action]/{token}")]
        public void ValidatePasswordResetLink(string token)
        {
            if (HttpContext.Session.GetString("user") == null)
            {
                Response.Redirect("/ForgotPassword?success=false&message=session");
                return;
            }

            if (token == null)
            {
                Response.Redirect("/ForgotPassword?success=false&message=token");
                return;
            }

            if(token.Equals(""))
            {
                Response.Redirect("/ForgotPassword?success=false&message=token");
                return;
            }

            

            SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("SQL"));
            sqlConnection.Open();
            Models.User user = sqlConnection.Query<Models.User>("select * from [user] where email = @email", new { @email = HttpContext.Session.GetString("user") }).First();
            sqlConnection.Close();
            string[] arrayVal = userTokens.GetValueOrDefault(user.firstName + " " + user.lastName);
            if(arrayVal == null)
            {
                Response.Redirect("/ForgotPassword?success=false&message=session");
                return;
            }

            if (!arrayVal[0].Equals(token))
            {
                HttpContext.Session.SetString("user", "false");
                Response.Redirect("/ForgotPassword?success=false&message=token");
            }
            else
            {
                userTokens.Remove(user.firstName + " " + user.lastName);
                HttpContext.Session.SetString("user", "true");
                Response.Redirect("/ForgotPassword?success=true&message=accepted");
            }
        }

        [HttpPost("[action]")]
        public void SetPassword([FromForm] Binders.UserLogin req)
        {
            if (HttpContext.Session.GetString("user") == null)
            {
                Response.Redirect("/ForgotPassword?success=false&message=session");
                return;
            }
            else if (HttpContext.Session.GetString("user").Equals("false"))
            {
                Response.Redirect("/ForgotPassword?success=false&message=token");
                return;
            }
            else
            {
                SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("SQL"));
                sqlConnection.Open();
                int numRows = sqlConnection.Execute("update [user] set password = @password where email = @username", req);
                if(numRows == 1)
                {
                    HttpContext.Session.Clear();
                    Response.Redirect("/Login?success=true&message=reset");
                }
                else
                {
                }
            }
        }
    }
}
