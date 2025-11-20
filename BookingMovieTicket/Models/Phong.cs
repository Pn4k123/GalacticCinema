using System;
using System.Collections.Generic;

namespace BookingMovieTicket.Models;

public partial class Phong
{
    public string MaPhong { get; set; } = null!;

    public string MaRap { get; set; } = null!;

    public string SoPhong { get; set; } = null!;

    public int TongSoGhe { get; set; }

    public string TrangThai { get; set; } = null!;

    public virtual ICollection<Ghe> Ghes { get; set; } = new List<Ghe>();

    public virtual Rap MaRapNavigation { get; set; } = null!;

    public virtual ICollection<SuatChieu> SuatChieus { get; set; } = new List<SuatChieu>();
}
