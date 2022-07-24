using System;
using System.Collections.Generic;

namespace Models
{
    public partial class Category
    {
        public Category()
        {
            ItemCharacteristics = new HashSet<ItemCharacteristic>();
        }

        public int Id { get; set; }
        public string? Name { get; set; }

        public virtual ICollection<ItemCharacteristic> ItemCharacteristics { get; set; }
    }
}
