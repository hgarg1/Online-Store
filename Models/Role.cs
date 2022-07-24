using System;
using System.Collections.Generic;

namespace Models
{
    public partial class Role
    {
        public Role()
        {
            Users = new HashSet<User>();
        }

        public int Id { get; set; }
        public string? RoleName { get; set; }
        public int PermissionsFk { get; set; }

        public virtual Permission PermissionsFkNavigation { get; set; } = null!;
        public virtual ICollection<User> Users { get; set; }
    }
}
