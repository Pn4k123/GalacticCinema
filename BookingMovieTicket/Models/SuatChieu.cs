using System;
using System.Collections.Generic;

namespace BookingMovieTicket.Models;

public partial class SuatChieu
{
    public string MaSuatChieu { get; set; } = null!;

    public string MaPhong { get; set; } = null!;

    public DateOnly NgayChieu { get; set; }

    public TimeOnly GioChieu { get; set; }

    public string TrangThai { get; set; } = null!;

    public virtual Phong MaPhongNavigation { get; set; } = null!;

    public virtual ICollection<Ve> Ves { get; set; } = new List<Ve>();
}
