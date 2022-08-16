using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Binders;
using Online_Store.Filters;
using Dapper;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Azure.Storage.Blobs;
using System.Diagnostics;
using Models;

namespace Online_Store.controllers.api
{

    [Route("/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private OnlineStore ctx = new OnlineStore();
        private static Dictionary<string, string[]> userTokens = new Dictionary<string, string[]>();

        private IConfiguration _configuration;
        public AuthController(IConfiguration configuration) //allows us to get connection string
        {
            _configuration = configuration;
        }

        [AcceptVerbs("Get","Post",Route = "[Action]")]
        public void Login([FromForm] Binders.UserLogin req)
        {
            SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("SQL"));
            sqlConnection.Open();


            Models.User? user = ctx.Users.Where(u=>u.Email.Equals(req.username) && u.Password.Equals(Encoding.ASCII.GetString(MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(req.password))))).FirstOrDefault();

            if (user == null)
            {
                Response.Redirect("/Login?success=false");
                return;
            }

            if (user.Role <= ctx.Roles.Where(r => r.RoleName.Equals("Admnistrator")).First().Id)
            {
                string redirectUrl = "";
                if (req.role != null && req.role == 5) //on means the radio was checked
                {
                    redirectUrl = "/Index";
                }
                else if (req.role != null && req.role == 3)
                {
                    redirectUrl = "/Admin/Index";
                }
                else
                {
                    Response.Redirect("/Login?success=false&message=choose");
                    sqlConnection.Close();
                    return;
                }

                if (user.EmailVerified.Equals("false"))
                {
                    HttpContext.Session.SetString("user", JsonSerializer.Serialize(user));
                    SendEmailValidation(user.Email, true);
                    Response.Redirect("/Login?success=false&message=email");
                }
                else if (user.EmailVerified.Equals("true"))
                {
                    user.LastLogin = DateTime.Now.ToString();
                    ctx.Update(user);
                    ctx.SaveChanges();
                    HttpContext.Session.SetString("user", JsonSerializer.Serialize(user, new JsonSerializerOptions()
                    {
                        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
                    }));//set session object for authentication into restircted pages
                    Response.Redirect(redirectUrl);
                }
                sqlConnection.Close();
            }
            else
            {
                if (user.EmailVerified.Equals("true"))
                {
                    user.LastLogin = DateTime.Now.ToString();
                    ctx.Update(user);
                    ctx.SaveChanges();
                    HttpContext.Session.SetString("user", JsonSerializer.Serialize(user));//set session object for authentication into restircted pages
                    Response.Redirect("/Index");
                }
                else if (user.EmailVerified.Equals("false"))
                {
                    sqlConnection.Close();
                    HttpContext.Session.SetString("user", JsonSerializer.Serialize(user));
                    SendEmailValidation(user.Email, true);
                    Response.Redirect("/Login?success=false&message=email");
                }
                else
                {
                    sqlConnection.Close();
                    Response.Redirect("/Login?success=false");//add error so login can display such message
                }
            }
        }

        [HttpPost("[action]")]
        public void Logout(bool bypassCacheCheck = false, bool redirect = true)
        {

            AuthFilter auth = new AuthFilter(_configuration);//pass through auth, providing means without adittional calls
            if (!bypassCacheCheck && !auth.isValid(HttpContext.Session.GetString("user")))
            {
                string referer = ((string)HttpContext.Request.Headers["referer"]);
                Response.Redirect(referer.Substring(0, referer.IndexOf("?") == -1 ? referer.Length : referer.IndexOf("?")) + "?success=false&message=0");
                return;
            }

            if (!bypassCacheCheck)
            {
                HttpContext.Session.Clear();
            }

            if (redirect)
            {
                Response.Redirect("/Login");
            }
        }

        [HttpPost("[action]")]
        public void Signup([FromForm] Binders.UserSignup req, [FromForm] IFormFile userProfilePicture)
        {
            if (!(String.Equals(req.password, req.confPassword)))
            {
                Response.Redirect("/Signup?success=false&message=0");
                return;
            }
            SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("SQL"));
            sqlConnection.Open();
            Models.User? isExisting = sqlConnection.Query<Models.User>("select * from [user] where email = @email", req).SingleOrDefault();
            if (isExisting == null)
            {
                req.lastLogin = null;
                req.password = Encoding.ASCII.GetString(MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(req.password)));
                int rowsAffected = sqlConnection.Execute("exec os_sp_addUser @firstName, @lastName, @email, '', @password, @address, @age, 'false', 'User', @sex, @ethnicity", req); //inserts bound object into data
                                                                                                                                                                                             //statement above is sysnonymous to a prepared statement
                sqlConnection.Close();

                BlobContainerClient container = new BlobContainerClient(_configuration.GetConnectionString("AZBlob"), "userprofileimages");
                BlobClient client = container.GetBlobClient(req.email + "_ProfilePicture.image");
                client.Upload(userProfilePicture.OpenReadStream(), true, default);

                //partial cache save, only enough for send email to work
                HttpContext.Session.SetString("user", JsonSerializer.Serialize(new Models.User() { FirstName = req.firstName, LastName = req.lastName }));
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
            if (cache == null)
            {
                Response.Redirect("/Email?success=false&message=0"); //user doesn't exist
                return;
            }

            Models.User reqUser = JsonSerializer.Deserialize<Models.User>(HttpContext.Session.GetString("user"));
            string?[] arrayVal = { };
            if (userTokens.TryGetValue(reqUser.FirstName + " " + reqUser.LastName, out arrayVal))//handles if user requests new link, voids old one
            {
                Console.WriteLine("Token Provided: " + token + " Token Found in DB: " + arrayVal[0]);
                if (!arrayVal[0].Equals(token))
                {
                    Response.Redirect("/Email?success=false&message=1"); //wrong token
                    return;
                }
                arrayVal[0] = null;
                userTokens.Remove(reqUser.FirstName + " " + reqUser.LastName);

                SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("SQL"));
                sqlConnection.Execute("update [user] set emailVerified = 'true' where email = @email", new { @email = reqUser.Email });
                sqlConnection.Execute("update [user] set email = @emailNew where email = @emailOld", new { @emailNew = arrayVal[1], @emailOld = reqUser.Email });
                sqlConnection.Close();
                reqUser.EmailVerified = "true";
                reqUser.Email = arrayVal[0];
                HttpContext.Session.SetString("user", JsonSerializer.Serialize(reqUser)); //updates cache


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



            Models.User reqUser = JsonSerializer.Deserialize<Models.User>(cache);

            Models.User user = new SqlConnection(_configuration.GetConnectionString("SQL")).Query<Models.User>("select * from [user] where email = @email",
                new { @email = email }
            ).First();

            if (user.EmailVerified.Equals("true"))
            {
                if (!noRedirect)
                {
                    Response.Redirect("/");
                    return;
                }
                else
                {
                    return;
                }
            }

            String randomKey = GetUniqueKey(32);
            string[]? arrayVal = { };
            if (userTokens.TryGetValue(reqUser.FirstName + " " + reqUser.LastName, out arrayVal))//handles if user requests new link, voids old one
            {
                arrayVal[0] = null;
                userTokens.Remove(reqUser.FirstName + " " + reqUser.LastName);
                userTokens.Add(reqUser.FirstName + " " + reqUser.LastName, new string[] { randomKey, email });
                //updates uer db and cache using prior token email pair to as old email
                new SqlConnection(_configuration.GetConnectionString("SQL")).Execute("update [user] set email = @emailNew where email = @emailOld", new { @emailNew = email, @emailOld = arrayVal[1] });
                reqUser.Email = email;
                HttpContext.Session.SetString("user", JsonSerializer.Serialize(reqUser));
            }
            else
            {
                userTokens.Add(reqUser.FirstName + " " + reqUser.LastName, new string[] { randomKey, email });
                //updates uer db and cache using cache as old email
                new SqlConnection(_configuration.GetConnectionString("SQL")).Execute("update [user] set email = @emailNew where email = @emailOld", new { @emailNew = email, @emailOld = reqUser.Email });
                reqUser.Email = email;
                HttpContext.Session.SetString("user", JsonSerializer.Serialize(reqUser));
            }

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential("hgarg1@terpmail.umd.edu", _configuration["SMTP:Password"]);
            smtpClient.Send(new MailMessage("hgarg1@terpmail.umd.edu", email, "Your Access Link | Online Store", $"Please use this url in the SAME browser when confirming your email.\nLink: {Request.Scheme}://{Request.Host}/Auth/ValidateEmail/{randomKey}\nThe link will expire in 5 hours.\nThanks for shopping with us!\nSincerely, Online Store Team"));
            Task.Delay(new TimeSpan(5, 0, 0)).ContinueWith(action =>
            {
                userTokens.Remove(reqUser.FirstName + " " + reqUser.LastName);
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
            Models.User user = null;
            try
            {
                user = sqlConnection.Query<Models.User>("select * from [user] where email = @email", new { @email = req.username }).First();
            }
            catch (InvalidOperationException e)
            {
                Response.Redirect("/ForgotPassword?success=false&message=user");
                return;
            }
            if (user == null)
            {
                Response.Redirect("/ForgotPassword?success=false&message=user");
                return;
            }
            sqlConnection.Close();

            string[] arrayVal = null;
            if (userTokens.TryGetValue(user.FirstName + " " + user.LastName, out arrayVal))
            {
                userTokens.Remove(user.FirstName + " " + user.LastName);
                arrayVal = new string[] { GetUniqueKey(32), req.username };
                userTokens.Add(user.FirstName + " " + user.LastName, arrayVal);
            }
            else
            {
                arrayVal = new string[] { GetUniqueKey(32), req.username };
                userTokens.Add(user.FirstName + " " + user.LastName, arrayVal);
            }

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential("hgarg1@terpmail.umd.edu", "Deepak@2003_101");
            smtpClient.Send(new MailMessage("hgarg1@terpmail.umd.edu", req.username, "Your Password Reset Link Link | Online Store", $"Please use this url in the SAME browser when resseting your password.\nLink: {Request.Scheme}://{Request.Host}/Auth/ValidatePasswordResetLink/{arrayVal[0]}\nThe link will expire in 5 hours.\nThanks for shopping with us!\nSincerely, Online Store Team"));

            HttpContext.Session.SetString("user", JsonSerializer.Serialize(new Models.User() { Email = req.username }));

            Task.Delay(new TimeSpan(5, 0, 0)).ContinueWith(action =>
            {
                userTokens.Remove(user.FirstName + " " + user.LastName);
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

            if (token.Equals(""))
            {
                Response.Redirect("/ForgotPassword?success=false&message=token");
                return;
            }



            SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("SQL"));
            sqlConnection.Open();
            String cache = JsonSerializer.Deserialize<Models.User>(HttpContext.Session.GetString("user")).Email;
            Models.User user = sqlConnection.Query<Models.User>("select * from [user] where email = @email", new { @email = cache }).First();
            sqlConnection.Close();
            string[] arrayVal = userTokens.GetValueOrDefault(user.FirstName + " " + user.LastName);
            if (arrayVal == null)
            {
                Response.Redirect("/ForgotPassword?success=false&message=session");
                return;
            }

            if (!arrayVal[0].Equals(token))
            {
                Response.Redirect("/ForgotPassword?success=false&message=token");
            }
            else
            {
                HttpContext.Session.SetString("user", JsonSerializer.Serialize(new Models.User() { Email = cache }));
                userTokens.Remove(user.FirstName + " " + user.LastName);
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
            else if (JsonSerializer.Deserialize<Models.User>(HttpContext.Session.GetString("user")).Email != null)
            {
                Response.Redirect("/ForgotPassword?success=false&message=token");
                return;
            }
            else
            {
                SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("SQL"));
                sqlConnection.Open();
                int numRows = sqlConnection.Execute("update [user] set password = @password where email = @username", req);
                if (numRows == 1)
                {
                    HttpContext.Session.Clear();
                    Response.Redirect("/Login?success=true&message=reset");
                }
                else
                {
                    HttpContext.Session.Clear();
                    Response.Redirect("/Login?success=true&message=reset");
                }
            }
        }
    }
}
