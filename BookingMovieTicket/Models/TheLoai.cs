using System;
using System.Collections.Generic;

namespace BookingMovieTicket.Models;

public partial class TheLoai
{
    public string MaTheLoai { get; set; } = null!;

    public string TenTheLoai { get; set; } = null!;

    public virtual ICollection<Phim> MaPhims { get; set; } = new List<Phim>();
}
