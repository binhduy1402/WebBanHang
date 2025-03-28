using System.ComponentModel.DataAnnotations;

namespace WebBanHang.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required, StringLength(50)]
        public required string Name { get; set; }
    }
}
