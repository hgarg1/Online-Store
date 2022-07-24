using System;
using System.Collections.Generic;

namespace Models
{
    public partial class ItemCharacteristic
    {
        public ItemCharacteristic()
        {
            Items = new HashSet<Item>();
        }

        public int Id { get; set; }
        public string? Color { get; set; }
        public string? Width { get; set; }
        public string? Height { get; set; }
        public string? Description { get; set; }
        public string? Notes { get; set; }
        public int? Category { get; set; }

        public virtual Category? CategoryNavigation { get; set; }
        public virtual ICollection<Item> Items { get; set; }
    }
}
