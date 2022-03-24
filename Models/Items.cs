using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Items
    {
        public int Id { get; set; }
        public string pictureLocation { get; set; }
        public float Price { get; set; }
        public int Quantity { get; set; }
        public string Supplier { get; set; }
        public int? characteristic_fk { get; set; }
        public string name { get; set; }
    }
}
