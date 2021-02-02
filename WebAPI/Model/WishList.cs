using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Model
{
    public class WishList
    {
        public string Name;
        public ICollection<Product> list { get; set; }
    }
}
