using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanHang.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Giá không được để trống")]
        [Range(1, 1000000000, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal Price { get; set; }

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Danh mục là bắt buộc")]
        public int CategoryId { get; set; }

        public Category? Category { get; set; }
    }
}
