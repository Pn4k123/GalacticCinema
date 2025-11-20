using System;
using System.Collections.Generic;

namespace BookingMovieTicket.Models;

public partial class ThanhToan
{
    public string MaThanhToan { get; set; } = null!;

    public string PhuongThuc { get; set; } = null!;

    public DateTime ThoiGian { get; set; }

    public string TrangThai { get; set; } = null!;

    public virtual ICollection<DonDatVe> DonDatVes { get; set; } = new List<DonDatVe>();
}
