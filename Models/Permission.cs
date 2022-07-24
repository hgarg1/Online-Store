using System;
using System.Collections.Generic;

namespace Models
{
    public partial class Permission
    {
        public Permission()
        {
            Roles = new HashSet<Role>();
        }

        public int Id { get; set; }
        public string? Permission1 { get; set; }

        public virtual ICollection<Role> Roles { get; set; }
    }
}
