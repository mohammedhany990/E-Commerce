using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.DAL.Models
{
    public class OrderDetail : BaseEntity
    {

        public int OrderHeaderId { get; set; }

        public OrderHeader OrderHeader { get; set; }

        public int ProductId { get; set; }

        public Product Product { get; set; }

        public int Count { get; set; }

        public double Price { get; set; }
    }
}
