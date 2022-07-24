using Binders;
using System;
using System.Collections.Generic;

namespace Models
{
    public partial class User
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Email { get; set; } = null!;
        public string? LastLogin { get; set; }
        public string? Password { get; set; }
        public string? Address { get; set; }
        public int Id { get; set; }
        public int Age { get; set; }
        public string? EmailVerified { get; set; }
        public int? Role { get; set; }
        public int? Sex { get; set; }
        public int? Ethnicity { get; set; }

        public virtual Ethnicity? EthnicityNavigation { get; set; }
        public virtual Role? RoleNavigation { get; set; }
        public virtual Gender? SexNavigation { get; set; }

        public override bool Equals(object? obj)
        {
            if(obj == null) return false;

            if(obj is User)
            {
                User req = (User)obj;
                return 
                    req.FirstName.Equals(FirstName) && 
                    req.LastName.Equals(LastName) && 
                    req.Email.Equals(Email) && 
                    req.LastLogin.Equals(LastLogin) && 
                    req.Password.Equals(Password) && 
                    req.Address.Equals(Address) && 
                    req.EmailVerified.Equals(EmailVerified);
            }else if(obj is UserLogin)
            {
                UserLogin req = (UserLogin)obj;
                return Email.Equals(req.username) && Password.Equals(req.password);
            }
            return false;
        }
    }
}
