using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BookingMovieTicket.Models;

public partial class SuatChieu
{
    [DisplayName("Mã suất chiếu")]
    public string MaSuatChieu { get; set; } = null!;
    [DisplayName("Mã phòng")]
    public string MaPhong { get; set; } = null!;
    [DisplayName("Mã phim")]
    public string MaPhim { get; set; } = null!;
    [DisplayName("Ngày chiếu")]
    public DateOnly NgayChieu { get; set; }
    [DisplayName("Giờ chiếu")]
    public TimeOnly GioChieu { get; set; }
    [DisplayName("Trạng thái")]
    public string TrangThai { get; set; } = null!;

    public virtual Phim MaPhimNavigation { get; set; } = null!;

    public virtual Phong MaPhongNavigation { get; set; } = null!;

    public virtual ICollection<Ve> Ves { get; set; } = new List<Ve>();
}
