using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BookingMovieTicket.Models;

public partial class NguoiDung
{
    public string MaNd { get; set; } = null!;
    [DisplayName("Email")]
    public string Email { get; set; } = null!;
    [DisplayName("Mật khẩu")]
    public string MatKhau { get; set; } = null!;
    [DisplayName("Họ và tên")]
    public string HoTen { get; set; } = null!;
    [DisplayName("Giới tính")]
    public byte? GioiTinh { get; set; }
    [DisplayName("Ngày sinh")]
    public DateTime? NgaySinh { get; set; }
    [DisplayName("Số điện thoại")]
    public string Sdt { get; set; } = null!;

    public string? VaiTro { get; set; } = null!;

    public string? RandomKey { get; set; }

    public virtual ICollection<DonDatVe> DonDatVes { get; set; } = new List<DonDatVe>();
}
