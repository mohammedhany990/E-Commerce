using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.DAL.Models
{
    public class ShoppingCart :BaseEntity
    {
        [Range(1,100, ErrorMessage = "Enter a value between 1 : 100")]
        public int Count { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }

        [NotMapped]
        public double Price { get; set; }
    }
}
