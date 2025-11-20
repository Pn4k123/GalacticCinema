using System;
using System.Collections.Generic;

namespace BookingMovieTicket.Models;

public partial class Rap
{
    public string MaRap { get; set; } = null!;

    public string TenRap { get; set; } = null!;

    public string DiaChi { get; set; } = null!;

    public string TrangThai { get; set; } = null!;

    public virtual ICollection<Phong> Phongs { get; set; } = new List<Phong>();

    public virtual ICollection<Ve> Ves { get; set; } = new List<Ve>();
}
