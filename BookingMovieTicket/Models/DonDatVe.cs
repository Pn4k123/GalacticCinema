using System;
using System.Collections.Generic;

namespace BookingMovieTicket.Models;

public partial class DonDatVe
{
    public string MaDon { get; set; } = null!;

    public string MaNd { get; set; } = null!;

    public DateTime ThoiGianDat { get; set; }

    public decimal TongTien { get; set; }

    public string? TrangThai { get; set; }

    public virtual ICollection<ChiTietDonDatVe> ChiTietDonDatVes { get; set; } = new List<ChiTietDonDatVe>();

    public virtual NguoiDung MaNdNavigation { get; set; } = null!;

    public virtual ICollection<ThanhToan> ThanhToans { get; set; } = new List<ThanhToan>();
}
