using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.DAL.Models
{
    
    public class Product : BaseEntity
    {
        [Required]
        public string Title { get; set; }
       
        [Required]
        public string Description { get; set; }

        [Required]
        public string ISBN { get; set; }

        [Required]
        public string Author { get; set; }

        [Required]
        [Range(1, 1000)]
        public double ListPrice { get; set; }



        [Required]
        [Range(1,1000)]
        public double Price { get; set; }

        [Required]
        [Range(1, 1000)]
        public double Price50 { get; set; }

        [Required]
        [Range(1, 1000)]
        public double Price100 { get; set; }


        
        public int? CategoryId { get; set; }

        public Category Category { get; set; }



        public List<ProductImage>? ProductImages { get; set; }

    }

    
}
