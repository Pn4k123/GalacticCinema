using System;
using System.Collections.Generic;

namespace BookingMovieTicket.Models;

public partial class Ve
{
    public string MaVe { get; set; } = null!;

    public string MaPhim { get; set; } = null!;

    public string MaRap { get; set; } = null!;

    public string MaSuatChieu { get; set; } = null!;

    public string MaGhe { get; set; } = null!;

    public DateTime ThoiGianPhatHanh { get; set; }

    public string TrangThai { get; set; } = null!;

    public virtual ICollection<ChiTietDonDatVe> ChiTietDonDatVes { get; set; } = new List<ChiTietDonDatVe>();

    public virtual Ghe MaGheNavigation { get; set; } = null!;

    public virtual Phim MaPhimNavigation { get; set; } = null!;

    public virtual Rap MaRapNavigation { get; set; } = null!;

    public virtual SuatChieu MaSuatChieuNavigation { get; set; } = null!;
}
