using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BookingMovieTicket.Models;

public partial class TheLoai
{
    [DisplayName("Mã thể loại")]
    public string MaTheLoai { get; set; } = null!;
    [DisplayName("Tên thể loại")]
    public string TenTheLoai { get; set; } = null!;

    public virtual ICollection<Phim> MaPhims { get; set; } = new List<Phim>();
}
