using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Binders;
using System.Net.Mail;
using System.Net;
using Microsoft.Data.SqlClient;
using Dapper;
using Models;
using System.Text.Json;

namespace Online_Store.controllers.api
{
    [Route("api/")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private IConfiguration _configuration;
        public ApiController(IConfiguration _config)
        {
            _configuration = _config;
        }

        [HttpPost("[action]")]
        public void Email([FromForm] Email email)
        {

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential("hgarg1@terpmail.umd.edu", _configuration["SMTP:Password"]);
            smtpClient.Send(new MailMessage(email.From, "hgarg1@terpmail.umd.edu", $"Online Store | Form Submission: {email.UserName}","Inquiry Subject: " + email.Subject + "\n" + email.Body+$"\nReply to: {email.From} to get back in touch!"));
            Response.Redirect("/Contact?success=true");
        }
    }
}
