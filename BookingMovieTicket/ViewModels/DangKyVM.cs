
using System.ComponentModel;
using System;
using System.Collections.Generic;
using BookingMovieTicket.Models;
using System.ComponentModel.DataAnnotations;


namespace BookingMovieTicket.ViewModels
{
    public class DangKyVM
    {
        [DisplayName("Email")]
        [EmailAddress(ErrorMessage = "Chưa đúng định dạng Email")]
        [Required(ErrorMessage = "Vui lòng nhập Email")]
        public string Email { get; set; }
        [DisplayName("Mật khẩu")]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string MatKhau { get; set; } 
        [DisplayName("Họ và tên")]
        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        public string HoTen { get; set; } 
        [DisplayName("Giới tính")]
        public byte? GioiTinh { get; set; } = 1;
        [DisplayName("Ngày sinh")]
        [Required(ErrorMessage = "Vui lòng nhập ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? NgaySinh { get; set; }
        [DisplayName("Số điện thoại")]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string Sdt { get; set; } = null!;
      
    }
}
