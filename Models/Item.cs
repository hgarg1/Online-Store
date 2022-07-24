using System;
using System.Collections.Generic;

namespace Models
{
    public partial class Item
    {
        public int Id { get; set; }
        public string? PictureLocation { get; set; }
        public double? Price { get; set; }
        public int? Quantity { get; set; }
        public string? Supplier { get; set; }
        public int? CharacteristicFk { get; set; }
        public string? Name { get; set; }

        public virtual ItemCharacteristic? CharacteristicFkNavigation { get; set; }
    }
}
