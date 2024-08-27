using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Demo.PL.Models
{
    public class CategoryViewModel
    {

        public int Id { get; set; }
        [Required]
        [DisplayName("Category Name")]
        [MaxLength(30)]
        public string Name { get; set; }

        [DisplayName("Display Order")]
        [Range(1, 100, ErrorMessage = "Please enter number between 1-100")]
        [Required(ErrorMessage = "This field is required.")]
        public int DisplayOrder { get; set; }

    }
}
