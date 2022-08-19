using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Models;
using Dapper;
using System.Text.Json;
using Azure.Storage.Blobs;
using System.Text;
using System.Security.Cryptography;

namespace Online_Store.controllers.api
{
    [Route("/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private IConfiguration _configuration;
        private AuthController authNeeds;
        private OnlineStore ctx = new OnlineStore();
        public UserController(IConfiguration configuration)
        {
            _configuration = configuration;
            authNeeds = new AuthController(_configuration); //pass through auth
        }

        [HttpPost("[action]")]
        public void Update([FromForm]User req, [FromForm] IFormFile? userProfileImage)
        {
            SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("SQL"));
            sqlConnection.Open();

            User? cache = JsonSerializer.Deserialize<Models.User>(HttpContext.Session.GetString("user"), new JsonSerializerOptions()
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
            });
            sqlConnection.Close();
            HttpContext.Session.SetString("user", JsonSerializer.Serialize(req)); //updates cache

            if(userProfileImage != null)
            {
                BlobContainerClient container = new BlobContainerClient(_configuration.GetConnectionString("AZBlob"), "userprofileimages");
                BlobClient client = container.GetBlobClient(req.Email + "_ProfilePicture.image");
                client.Upload(userProfileImage.OpenReadStream(), true, default);
            }
            
            req.Password = Encoding.ASCII.GetString(MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(req.Password)));

            if (!String.Equals(req.Email, cache.Email))
            {
                sqlConnection.Execute("exec os_sp_updateUser @FirstName, @LastName, @Email, @LastLogin, @Password, @Address, @Age, 'false', @Role, @Sex, @Ethnicity;", req);
                sqlConnection.Close();
                req.EmailVerified = "false";
                HttpContext.Session.SetString("user",JsonSerializer.Serialize(req));
                authNeeds.SendEmailValidation(req.Email, true);
                authNeeds.Logout(true, false);
                Response.Redirect("/Login?success=false&message=email");
                return;
            }
            else
            {
                sqlConnection.Execute("exec os_sp_updateUser @FirstName, @LastName, @Email, @LastLogin, @Password, @Address, @Age, 'true', @Role, @Sex, @Ethnicity;", req);
                sqlConnection.Close();
                req.EmailVerified = "true";
                HttpContext.Session.SetString("user", JsonSerializer.Serialize(req));
                Response.Redirect("/Settings");
                return;
            }
        }

        [HttpPost("[action]")]
        public String Delete()
        {
            SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("SQL"));
            sqlConnection.Open();

            User? req = JsonSerializer.Deserialize<Models.User>(HttpContext.Session.GetString("user"), new JsonSerializerOptions()
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
            });

            sqlConnection.Execute("delete from [user] where email = @email AND password = @password", new { 
                @email = req.Email, @password = req.Password 
            });
            sqlConnection.Close();

            HttpContext.Session.Clear();
            return "/Login";    
        }

        [HttpGet("[Action]")]
        public Boolean IsAdmin()
        {
            if(HttpContext.Session.GetString("user") == null)
            {
               return false;
            }
            
            User? user = JsonSerializer.Deserialize<User>(HttpContext.Session.GetString("user"), new JsonSerializerOptions()
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
            });

            OnlineStore ctx = new OnlineStore();
            if (user.Role <= ctx.Roles.Where(r=>r.RoleName.Equals("Admnistrator")).First().Id)
            {
                return true;
            }
            return false;
        }

        [HttpGet("[Action]")]
        public IEnumerable<Gender> GetGenders()
        {
            return ctx.Genders.AsList();
        }

        [HttpGet("[Action]")]
        public IEnumerable<Ethnicity> GetEthnicities()
        {
            return ctx.Ethnicities.AsList();
        }

        [HttpGet("[Action]")]
        public List<User> GetUsers()
        {
            return ctx.Users.AsList();
        }
    }
}
