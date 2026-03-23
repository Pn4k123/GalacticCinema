using System;
using System.Collections.Generic;

namespace BookingMovieTicket.Models;

public partial class ThanhToan
{
    public string MaDon { get; set; } = null!;

    public string PhuongThuc { get; set; } = null!;

    public DateTime ThoiGian { get; set; }

    public string TrangThai { get; set; } = null!;

    public virtual DonDatVe MaDonNavigation { get; set; } = null!;
}
