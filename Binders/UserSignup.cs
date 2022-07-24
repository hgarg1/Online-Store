using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binders
{
    public class UserSignup //used when a signup request is intiated, form data is bound to C# fields
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string? lastLogin { get; set; }
        public string password { get; set; }
        public string confPassword { get; set; }
        public string address { get; set; }
        public string sex { get; set; }
        public int age { get; set; }
        public string ethnicity { get; set; }
        public string role { get; set; }
    }
}
