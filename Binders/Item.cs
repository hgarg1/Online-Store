using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binders
{
    public class Item
    {
        public int @Id { get; set; }
        public float @Price { get; set; }
        public int @Quantity { get; set; }
        public string? @supplier { get; set; }
        public string? @name { get; set; }
        public string? @width { get; set; }
        public string? @height { get; set; }
        public string? @color { get; set; }
        public string? @category { get; set; }
        public string? @description { get; set; }
        public string? @notes { get; set; }

        public IFormFile[] images { get; set; }
    }
}
