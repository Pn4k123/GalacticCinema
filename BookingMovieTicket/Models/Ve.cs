using System;
using System.Collections.Generic;

namespace BookingMovieTicket.Models;

public partial class Ve
{
    public string MaVe { get; set; } = null!;

    public string MaSuatChieu { get; set; } = null!;

    public string MaGhe { get; set; } = null!;

    public string? TrangThai { get; set; }

    public DateTime? ThoiGianPhatHanh { get; set; }

    public virtual ICollection<ChiTietDonDatVe> ChiTietDonDatVes { get; set; } = new List<ChiTietDonDatVe>();

    public virtual Ghe MaGheNavigation { get; set; } = null!;

    public virtual SuatChieu MaSuatChieuNavigation { get; set; } = null!;
}
