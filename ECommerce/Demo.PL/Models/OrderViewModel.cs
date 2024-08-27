using Demo.DAL.Models;

namespace Demo.PL.Models
{
    public class OrderViewModel
    {
        public OrderHeader OrderHeader { get; set; }
        public IEnumerable<OrderDetail> OrderDetails { get; set; }
    }
}
