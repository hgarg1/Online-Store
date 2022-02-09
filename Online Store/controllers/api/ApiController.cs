using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Binders;
using System.Net.Mail;
using System.Net;

namespace Online_Store.controllers.api
{
    [Route("api/")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        [HttpPost("[action]")]
        public void Email([FromForm] Email email)
        {

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential("hgarg1@terpmail.umd.edu", "Deepak@2003_101");
            smtpClient.Send(new MailMessage(email.From, "hgarg1@terpmail.umd.edu", $"Online Store | Form Submission: {email.UserName}",email.Body+$"\nReply to: {email.From} to get back in touch!"));
            Response.Redirect("/Contact?success=true");
        }
    }
}
