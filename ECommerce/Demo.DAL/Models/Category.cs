using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Demo.DAL.Models
{
    public class Category : BaseEntity
    {
        [Required]
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
    }



}
