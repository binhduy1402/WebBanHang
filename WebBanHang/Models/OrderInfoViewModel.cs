using System.ComponentModel.DataAnnotations;
namespace WebBanHang.Models

{
    public class OrderInfoViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone]
        public string Phone { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn hình thức thanh toán")]
        public string PaymentMethod { get; set; }
    }
}
