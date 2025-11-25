using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BookingMovieTicket.ViewModels
{
    public class DangNhapVM
    {
        [DisplayName("Email")]
        [EmailAddress(ErrorMessage = "Chưa đúng định dạng Email")]
        [Required(ErrorMessage = "Vui lòng nhập Email")]
        public string Email { get; set; }
        [DisplayName("Mật khẩu")]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; }
    }
}
