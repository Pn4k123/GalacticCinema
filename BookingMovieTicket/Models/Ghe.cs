using System;
using System.Collections.Generic;

namespace BookingMovieTicket.Models;

public partial class Ghe
{
    public string MaGhe { get; set; } = null!;

    public string MaPhong { get; set; } = null!;

    public int SoGhe { get; set; }

    public string LoaiGhe { get; set; } = null!;

    public string TrangThai { get; set; } = null!;

    public virtual Phong MaPhongNavigation { get; set; } = null!;

    public virtual ICollection<Ve> Ves { get; set; } = new List<Ve>();
}
