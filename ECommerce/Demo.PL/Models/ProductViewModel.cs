using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Demo.DAL.Models;

namespace Demo.PL.Models
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string ISBN { get; set; }

        [Required]
        public string Author { get; set; }

        [Required]
        [DisplayName("List Price")]
        [Range(1, 1000)]
        public double ListPrice { get; set; }



        [Required]
        [DisplayName("Price for 1-50")]
        [Range(1, 1000)]
        public double Price { get; set; }

        [Required]
        [DisplayName("Price for 50+")]
        [Range(1, 1000)]
        public double Price50 { get; set; }

        [Required]
        [DisplayName("Price for 100+")]
        [Range(1, 1000)]
        public double Price100 { get; set; }

        public int? CategoryId { get; set; }

        [Required]
        public Category Category { get; set; }


        public List<ProductImage>? ProductImages { get; set; }

    }
}
