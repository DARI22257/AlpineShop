using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlpineShop.Models
{
    public class Cart
    {
        public string ProductName { get; set; } 
        public string Category { get; set; } 
        public decimal Price { get; set; }
        public string ImageFile { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
