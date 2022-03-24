using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class ItemCharacteristic
    {
        public int id { get; set; }
        public string category { get; set; }
        public string color { get; set; }
        public double width { get; set; }
        public double height { get; set; }
        public string description { get; set; }
        public string notes { get; set; }
    }
}
