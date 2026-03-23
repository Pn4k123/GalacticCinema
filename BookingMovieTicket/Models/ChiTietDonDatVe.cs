using System;
using System.Collections.Generic;

namespace BookingMovieTicket.Models;

public partial class ChiTietDonDatVe
{
    public string MaDon { get; set; } = null!;

    public string MaVe { get; set; } = null!;

    public decimal GiaVe { get; set; }

    public virtual DonDatVe MaDonNavigation { get; set; } = null!;

    public virtual Ve MaVeNavigation { get; set; } = null!;
}
