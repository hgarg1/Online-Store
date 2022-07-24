using System;
using System.Collections.Generic;

namespace Models
{
    public partial class Ethnicity
    {
        public Ethnicity()
        {
            Users = new HashSet<User>();
        }

        public int Id { get; set; }
        public string? Ethnicity1 { get; set; }

        public virtual ICollection<User> Users { get; set; }
    }
}
