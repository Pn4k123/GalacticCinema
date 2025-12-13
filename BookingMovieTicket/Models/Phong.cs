using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BookingMovieTicket.Models;

public partial class Phong
{
    [DisplayName("Mã phòng")]
    public string MaPhong { get; set; } = null!;
    [DisplayName("Mã rạp")]
    public string MaRap { get; set; } = null!;
    [DisplayName("Tên phòng")]
    public string TenPhong { get; set; } = null!;
    [DisplayName("Trạng thái")]
    public string TrangThai { get; set; } = null!;

    public virtual ICollection<Ghe> Ghes { get; set; } = new List<Ghe>();

    public virtual Rap MaRapNavigation { get; set; } = null!;

    public virtual ICollection<SuatChieu> SuatChieus { get; set; } = new List<SuatChieu>();
}
