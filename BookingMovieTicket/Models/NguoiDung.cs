using System;
using System.Collections.Generic;

namespace BookingMovieTicket.Models;

public partial class NguoiDung
{
    public string MaNd { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public byte GioiTinh { get; set; }

    public DateOnly NgaySinh { get; set; }

    public string Sdt { get; set; } = null!;

    public string VaiTro { get; set; } = null!;

    public virtual ICollection<DonDatVe> DonDatVes { get; set; } = new List<DonDatVe>();
}
